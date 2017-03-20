/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Plugins.Web.
 *
 * Zongsoft.Plugins.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Plugins.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Plugins.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Http.Dispatcher;
using System.Web.Http.Controllers;

namespace Zongsoft.Plugins.Web
{
	public class Workbench : Zongsoft.Plugins.WorkbenchBase
	{
		#region 构造函数
		internal Workbench(ApplicationContext applicationContext) : base(applicationContext)
		{
		}
		#endregion

		#region 公共属性
		public WebConfiguration Web
		{
			get
			{
				return WebConfiguration.Instance;
			}
		}
		#endregion

		#region 重写方法
		protected override void OnStart(string[] args)
		{
			this.PluginContext.PluginTree.Mount(PluginPath.Combine(this.PluginContext.Settings.WorkbenchPath, "Web"), this.Web);
			this.PluginContext.PluginTree.Mount(PluginPath.Combine(this.PluginContext.Settings.WorkbenchPath, "Web", "ViewEngines"), this.Web.ViewEngines);
			this.PluginContext.PluginTree.Mount(PluginPath.Combine(this.PluginContext.Settings.WorkbenchPath, "Web", "Api"), this.Web.Api);

			var routeProvider = this.Web.RouteProvider;

			if(routeProvider != null)
			{
				foreach(var token in routeProvider.GetRoutes())
				{
					var route = token.ToRoute();

					if(route is RouteBase)
						RouteTable.Routes.Add((RouteBase)route);
					else if(route is IHttpRoute)
						GlobalConfiguration.Configuration.Routes.Add(token.Name, (IHttpRoute)route);
				}
			}

			//更改序列化器的默认设置
			GlobalConfiguration.Configuration.Formatters.XmlFormatter.UseXmlSerializer = true;
			var contractResolver = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver as Newtonsoft.Json.Serialization.DefaultContractResolver;
			if(contractResolver != null)
				contractResolver.IgnoreSerializableAttribute = true;

			//调用基类同名方法，以启动工作台下Startup下的所有工作者
			base.OnStart(args);
		}
		#endregion

		#region 嵌套子类
		public class WebConfiguration
		{
			#region 单例字段
			public static readonly WebConfiguration Instance = new WebConfiguration();
			#endregion

			#region 成员字段
			private System.Web.Mvc.IFilterProvider _filterProvider;
			#endregion

			#region 私有构造
			private WebConfiguration()
			{
			}
			#endregion

			#region 公共属性
			public HttpConfiguration Api
			{
				get
				{
					return HttpConfiguration.Instance;
				}
			}

			public Zongsoft.Web.Routing.IRouteProvider RouteProvider
			{
				get;
				set;
			}

			/// <summary>
			/// 获取或设置当前应用的<see cref="IControllerFactory"/>控制器工厂对象。
			/// </summary>
			public IControllerFactory ControllerFactory
			{
				get
				{
					return System.Web.Mvc.ControllerBuilder.Current.GetControllerFactory();
				}
				set
				{
					if(value == null)
						throw new ArgumentNullException();

					System.Web.Mvc.ControllerBuilder.Current.SetControllerFactory(value);
				}
			}

			/// <summary>
			/// 获取或设置当前应用的<seealso cref="System.Web.Mvc.IFilterProvider"/>过滤器提供程序。
			/// </summary>
			public System.Web.Mvc.IFilterProvider FilterProvider
			{
				get
				{
					return _filterProvider;
				}
				set
				{
					if(value == null)
						throw new ArgumentNullException();

					var filterProvider = System.Threading.Interlocked.Exchange(ref _filterProvider, value);

					if(filterProvider != null)
						FilterProviders.Providers.Remove(filterProvider);

					FilterProviders.Providers.Add(value);
				}
			}

			/// <summary>
			/// 获取当前应用的<see cref="ViewEngineCollection"/>视图引擎集合。
			/// </summary>
			public ViewEngineCollection ViewEngines
			{
				get
				{
					return System.Web.Mvc.ViewEngines.Engines;
				}
			}
			#endregion
		}

		public class HttpConfiguration
		{
			#region 单例字段
			public static readonly HttpConfiguration Instance = new HttpConfiguration();
			#endregion

			#region 成员字段
			private System.Web.Http.Filters.IFilterProvider _filterProvider;
			#endregion

			#region 私有构造
			private HttpConfiguration()
			{
			}
			#endregion

			#region 公共属性
			public IHttpActionSelector ActionSelector
			{
				get
				{
					return (IHttpActionSelector)GlobalConfiguration.Configuration.Services.GetService(typeof(IHttpActionSelector));
				}
				set
				{
					if(value == null)
						throw new ArgumentNullException();

					GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpActionSelector), value);
				}
			}

			public IHttpControllerSelector ControllerSelector
			{
				get
				{
					return (IHttpControllerSelector)GlobalConfiguration.Configuration.Services.GetService(typeof(IHttpControllerSelector));
				}
				set
				{
					if(value == null)
						throw new ArgumentNullException();

					GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), value);
				}
			}

			public System.Web.Http.Filters.IFilterProvider FilterProvider
			{
				get
				{
					return _filterProvider;
				}
				set
				{
					if(value == null)
						throw new ArgumentNullException();

					var filterProvider = System.Threading.Interlocked.Exchange(ref _filterProvider, value);

					if(filterProvider != null)
						GlobalConfiguration.Configuration.Services.Remove(typeof(System.Web.Http.Filters.IFilterProvider), filterProvider);

					GlobalConfiguration.Configuration.Services.Add(typeof(System.Web.Http.Filters.IFilterProvider), value);
				}
			}
			#endregion
		}
		#endregion
	}
}
