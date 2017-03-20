/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;

using Zongsoft.Plugins;

namespace Zongsoft.Web
{
	public abstract class PluginViewEngine : IViewEngine
	{
		#region 常量定义
		private const string ViewLocationCacheKeyFormat = "$Zongsoft.Plugins.Web.ViewLocationCacheEntry[{0}]:{1}@{2}";
		private const string ViewLocationCacheKeyPrefix_Master = "Master";
		private const string ViewLocationCacheKeyPrefix_Partial = "Partial";
		private const string ViewLocationCacheKeyPrefix_View = "View";
		#endregion

		#region 私有变量
		private static readonly string[] _emptyLocations = new string[0];
		#endregion

		#region 成员变量
		private PluginContext _pluginContext;
		private IViewLocationCache _viewLocationCache;
		private VirtualPathProvider _virtualProvider;
		private string[] _viewExtensions;
		private string[] _partialViewExtensions;
		private string[] _masterExtensions;
		#endregion

		#region 构造函数
		protected PluginViewEngine(PluginContext pluginContext)
		{
			if(pluginContext == null)
				throw new ArgumentNullException("pluginContext");

			_pluginContext = pluginContext;

			if(HttpContext.Current == null || HttpContext.Current.IsDebuggingEnabled)
				_viewLocationCache = DefaultViewLocationCache.Null;
			else
				_viewLocationCache = new DefaultViewLocationCache();
		}
		#endregion

		#region 公共属性
		public PluginContext PluginContext
		{
			get
			{
				return _pluginContext;
			}
		}

		public IViewLocationCache ViewLocationCache
		{
			get
			{
				return _viewLocationCache;
			}
			set
			{
				_viewLocationCache = value ?? DefaultViewLocationCache.Null;
			}
		}
		#endregion

		#region 保护属性
		protected string[] ViewExtensions
		{
			get
			{
				return _viewExtensions;
			}
			set
			{
				if(value == null || value.Length == 0)
					throw new ArgumentNullException();

				_viewExtensions = value;
			}
		}

		protected string[] PartialViewExtensions
		{
			get
			{
				return _partialViewExtensions;
			}
			set
			{
				if(value == null || value.Length == 0)
					throw new ArgumentNullException();

				_partialViewExtensions = value;
			}
		}

		protected string[] MasterExtensions
		{
			get
			{
				return _masterExtensions;
			}
			set
			{
				if(value == null || value.Length == 0)
					throw new ArgumentNullException();

				_masterExtensions = value;
			}
		}

		protected  VirtualPathProvider VirtualPathProvider
		{
			get
			{
				if(_virtualProvider == null)
					_virtualProvider = HostingEnvironment.VirtualPathProvider;

				return _virtualProvider;
			}
			set
			{
				_virtualProvider = value;
			}
		}
		#endregion

		#region 抽象方法
		protected abstract IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath);
		protected abstract IView CreatePartialView(ControllerContext controllerContext, string partialPath);
		#endregion

		#region 虚拟方法
		protected virtual bool FileExists(ControllerContext controllerContext, string virtualPath)
		{
			return this.VirtualPathProvider.FileExists(virtualPath);
		}
		#endregion

		#region 视图方法
		public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			if(controllerContext == null)
				throw new ArgumentNullException("controllerContext");

			if(string.IsNullOrEmpty(partialViewName))
				throw new ArgumentNullException("partialViewName");

			IEnumerable<string> searchedLocations;
			string partialPath = this.GetVirtualPath(controllerContext, partialViewName, _partialViewExtensions, ViewLocationCacheKeyPrefix_Partial, useCache, out searchedLocations);

			if(string.IsNullOrEmpty(partialPath))
				return new ViewEngineResult(searchedLocations);

			return new ViewEngineResult(this.CreatePartialView(controllerContext, partialPath), this);
		}

		public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			if(controllerContext == null)
				throw new ArgumentNullException("controllerContext");

			if(string.IsNullOrEmpty(viewName))
				throw new ArgumentNullException("viewName");

			IEnumerable<string> viewSearchedLocations;
			IEnumerable<string> masterSearchedLocations;

			string viewPath = this.GetVirtualPath(controllerContext, viewName, _viewExtensions, ViewLocationCacheKeyPrefix_View, useCache, out viewSearchedLocations);
			string masterPath = this.GetVirtualPath(controllerContext, masterName, _masterExtensions, ViewLocationCacheKeyPrefix_Master, useCache, out masterSearchedLocations);

			if(string.IsNullOrEmpty(viewPath) || (string.IsNullOrEmpty(masterPath) && !string.IsNullOrEmpty(masterName)))
				return new ViewEngineResult(viewSearchedLocations.Union(masterSearchedLocations));

			return new ViewEngineResult(this.CreateView(controllerContext, viewPath, masterPath), this);
		}

		public virtual void ReleaseView(ControllerContext controllerContext, IView view)
		{
			IDisposable disposable = view as IDisposable;

			if(disposable != null)
				disposable.Dispose();
		}
		#endregion

		#region 私有方法
		private string GetVirtualPath(ControllerContext controllerContext, string name, IEnumerable<string> extensions, string cacheKeyPrefix, bool useCache, out IEnumerable<string> searchedLocations)
		{
			//设置输出参数的默认值
			searchedLocations = _emptyLocations;

			//如果没有指定查找的名称则返回空(该调用方式在无母版页的视图中出现)
			if(string.IsNullOrWhiteSpace(name))
				return string.Empty;

			string controllerName = controllerContext.RouteData.GetRequiredString("controller");
			bool nameRepresentsPath = IsSpecificPath(name);
			string cacheKey = CreateCacheKey(cacheKeyPrefix, name, (nameRepresentsPath) ? string.Empty : controllerName);

			if(useCache)
				return _viewLocationCache.GetViewLocation(controllerContext.HttpContext, cacheKey);

			object nodePath;

			//从路由数据中获取由ControllerFactory解析出的当前Controller对应的插件路径
			if(!controllerContext.RouteData.DataTokens.TryGetValue("controller.path", out nodePath))
				return string.Empty;

			//查找当前控制器位于插件树中的节点
			var node = _pluginContext.PluginTree.Find((string)nodePath);

			//获取当前插件对应的视图位置
			IEnumerable<string> locations = this.GetSearchLocations((node == null ? null : node.Plugin), controllerName, name, extensions);

			foreach(string location in locations)
			{
				if(this.FileExists(controllerContext, location))
				{
					_viewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, location);
					return location;
				}
			}

			//设置输出参数为所有搜索的路径
			searchedLocations = locations.ToArray();

			return string.Empty;
		}

		private IEnumerable<string> GetSearchLocations(Plugin plugin, string controllerName, string name, IEnumerable<string> extensions)
		{
			string directoryName;
			List<string> locations = new List<string>();

			while(plugin != null)
			{
				//获取当前插件所在的目录
				directoryName = Path.GetDirectoryName(plugin.FilePath);
				directoryName = Path.Combine(directoryName, "views");

				this.SetSearchLocations(directoryName, controllerName, name, extensions, locations);
				this.SetSearchLocations(directoryName, null, name, extensions, locations);

				if(plugin.Manifest.Dependencies.Count > 0)
				{
					//遍历当前插件的依赖插件
					foreach(var dependency in plugin.Manifest.Dependencies)
					{
						directoryName = Path.GetDirectoryName(dependency.Plugin.FilePath);
						directoryName = Path.Combine(directoryName, "views");

						this.SetSearchLocations(directoryName, controllerName, name, extensions, locations);
						this.SetSearchLocations(directoryName, null, name, extensions, locations);
					}
				}

				//如果当前插件目录下没有找到指定的文件则查看父插件目录
				plugin = plugin.Parent;
			}

			//获取ASP.NET MVC默认的根视图目录
			directoryName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "views");

			//添加ASP.NET MVC默认的根视图目录下的各视图路径
			this.SetSearchLocations(directoryName, controllerName, name, extensions, locations);
			this.SetSearchLocations(directoryName, null, name, extensions, locations);

			return locations;
		}

		private void SetSearchLocations(string directoryName, string controllerName, string name, IEnumerable<string> extensions, IList<string> locations)
		{
			if(locations == null)
				throw new ArgumentNullException("locations");

			if(extensions == null)
			{
				locations.Add(this.CombinePath(directoryName, controllerName, name, null));
			}
			else
			{
				foreach(string extension in extensions)
				{
					locations.Add(this.CombinePath(directoryName, controllerName, name, extension));
				}
			}
		}

		private string CombinePath(string directoryName, string controllerName, string name, string extension)
		{
			string filePath;

			if(string.IsNullOrEmpty(extension))
			{
				if(string.IsNullOrEmpty(controllerName))
					filePath = name;
				else
					filePath = string.Format("{0}.{1}", controllerName, name);
			}
			else
			{
				if(extension[0] != '.')
					extension = '.' + extension;

				if(name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
				{
					if(string.IsNullOrEmpty(controllerName))
						filePath = name;
					else
						filePath = string.Format("{0}.{1}", controllerName, name);
				}
				else
				{
					if(string.IsNullOrEmpty(controllerName))
						filePath = name + extension;
					else
						filePath = string.Format("{0}.{1}{2}", controllerName, name, extension);
				}
			}

			//为了跨操作系统方便处理，一律对文件名部分做全小写处理。注意：目录部分必须保持真实大小写！
			if(string.IsNullOrEmpty(directoryName))
				filePath = filePath.ToLowerInvariant();
			else
				filePath = Path.Combine(directoryName, filePath.ToLowerInvariant());

			//将物理路径转换成虚拟路径
			return VirtualPathHelper.ToVirtualPath(filePath);
		}
		#endregion

		#region 私有静态
		private static bool IsSpecificPath(string name)
		{
			if(string.IsNullOrEmpty(name))
				return false;

			char c = name[0];
			return (c == '~' || c == '/');
		}

		private static string CreateCacheKey(string prefix, string name, string controllerName)
		{
			return string.Format(CultureInfo.InvariantCulture, ViewLocationCacheKeyFormat,
								prefix, name, controllerName);
		}
		#endregion
	}
}
