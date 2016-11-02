﻿using System;
using DotnetSpider.Core.Selector;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class SelectorTest
	{
		string _html = "<div><h1>test<a href=\"xxx\">aabbcc</a></h1></div>";
		string _html2 = "<div><a href='http://whatever.com/aaa'></a></div><div><a href='http://whatever.com/bbb'></a></div>";
		string _text = "{ \"store\": {\n\n\n" +
			"    \"book\": [ \n" +
			"      { \"category\":\n\n\n \"reference\",\n\n\n\n" +
			"        \"author\": \"Nigel Rees\",\n\n\n\n" +
			"        \"title\": \"Sayings of the Century\",\n" +
			"        \"price\": 8.95\n" +
			"      },\n" +
			"      { \"category\": \"fiction\",\n" +
			"        \"author\": \"Evelyn Waugh\",\n" +
			"        \"title\": \"Sword of Honour\",\n" +
			"        \"price\": 12.99,\n" +
			"        \"isbn\": \"0-553-21311-3\"\n" +
			"      }\n" +
			"    ],\n" +
			"    \"bicycle\": {\n" +
			"      \"color\": \"red\",\n" +
			"      \"price\": 19.95\n" +
			"    }\n" +
			"  }\n" +
			"}";

		[Fact]
		public void Regex()
		{
			Assert.Equal(Selectors.Regex("a href=\"(.*)\"").Select(_html), "a href=\"xxx\"");
			Assert.Equal(Selectors.Regex("(a href)=\"(.*)\"", 2).Select(_html), "xxx");
		}

		[Fact]
		public void Css()
		{
			Assert.Equal(Selectors.Css("div h1 a").Select(_html).OuterHtml, "<a href=\"xxx\">aabbcc</a>");
			Assert.Equal(Selectors.Css("div h1 a", "href").Select(_html), "xxx");
			Assert.Equal(Selectors.Css("div h1 a").Select(_html).InnerHtml, "aabbcc");
		}

		[Fact]
		public void Xpath()
		{
			Assert.Equal(Selectors.XPath("//a/@href").Select(_html), "xxx");
		}

		[Fact]
		public void JsonPath()
		{
			JsonPathSelector jsonPathSelector = new JsonPathSelector("$.store.book[*].author");
			var result1 = jsonPathSelector.Select(_text).ToString();
			var list1 = jsonPathSelector.SelectList(_text);
			Assert.Equal(result1, "Nigel Rees");
			Assert.True(list1.Contains("Nigel Rees"));
			Assert.True(list1.Contains("Evelyn Waugh"));

			jsonPathSelector = new JsonPathSelector("$.store.book[?(@.category == 'reference')]");
			var list2 = jsonPathSelector.SelectList(_text);
			var result2 = jsonPathSelector.Select(_text);

			Assert.Equal(result2, "{\r\n  \"category\": \"reference\",\r\n  \"author\": \"Nigel Rees\",\r\n  \"title\": \"Sayings of the Century\",\r\n  \"price\": 8.95\r\n}");
			Assert.Equal(list2[0], "{\r\n  \"category\": \"reference\",\r\n  \"author\": \"Nigel Rees\",\r\n  \"title\": \"Sayings of the Century\",\r\n  \"price\": 8.95\r\n}");
		}

		[Fact]
		public void RegexException()
		{
			Exception ex = Assert.Throws<ArgumentException>(() => new RegexSelector("\\d+("));
			Assert.NotNull(ex);
		}


		[Fact]
		public void TestRegexWithLeftBracketQuoted()
		{
			string regex = "\\(.+";
			string source = "(hello world";
			RegexSelector regexSelector = new RegexSelector(regex);
			string select = regexSelector.Select(source);
			Assert.Equal(select, source);
		}

		[Fact]
		public void XPath2()
		{
			Selectable selectable = new Selectable(_html2, "", ContentType.Html);
			var linksWithoutChain = selectable.Links().GetValues();
			ISelectable xpath = selectable.XPath("//div");
			var linksWithChainFirstCall = xpath.Links().GetValues();
			var linksWithChainSecondCall = xpath.Links().GetValues();
			Assert.Equal(linksWithoutChain.Count, linksWithChainFirstCall.Count);
			Assert.Equal(linksWithChainFirstCall.Count, linksWithChainSecondCall.Count);
		}

		[Fact]
		public void Selectable()
		{
			Selectable selectable = new Selectable(_html2, "", ContentType.Html);
			var links = selectable.XPath(".//a/@href").Nodes();
			Assert.Equal(links[0].GetValue(), "http://whatever.com/aaa");

			var links1 = selectable.XPath(".//a/@href").GetValue();
			Assert.Equal(links1, "http://whatever.com/aaa");
		}
	}
}
