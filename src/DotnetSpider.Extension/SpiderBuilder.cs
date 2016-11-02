#if NET_CORE
using System.Text;
#endif
using DotnetSpider.Core;
#if Test
using Newtonsoft.Json;
#endif

namespace DotnetSpider.Extension
{
	public abstract class EntitySpiderBuilder : IRunable
	{
		protected abstract EntitySpider GetEntitySpider();
#if NET_CORE
		protected EntitySpiderBuilder()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
#endif

		public virtual void Run(params string[] args)
		{
			var spider = GetEntitySpider();
#if Test
	// ת��JSON��ת����SpiderContext, ���ڲ���JsonSpiderContext�Ƿ�����
			string json = JsonConvert.SerializeObject(GetSpiderContext());
			ModelSpider spider = new ModelSpider(JsonConvert.DeserializeObject<JsonSpiderContext>(json).ToRuntimeContext());
#elif Publish
			//ModelSpider spider = new ModelSpider(context) {AfterSpiderFinished = AfterSpiderFinished};
#endif
			spider?.Run(args);
		}
	}
}