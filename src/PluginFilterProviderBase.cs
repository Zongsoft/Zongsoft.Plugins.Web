/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Linq;
using System.Collections.Generic;

namespace Zongsoft.Web
{
	/// <summary>
	/// 过滤器提供程序基类。
	/// </summary>
	/// <remarks>
	///		<para>全局过滤器挂载地址位于：/Workspace/Web/Filters 插件路径下；域级、控制器(Controller)和操作(Action)级的过滤器挂载地址位于：/Workspace/Web/Filters/{Area}.../{ControllerName} 插件路径下。</para>
	///		<para>控制器(Controller)和操作(Action)级过滤器的区别依据为构件属性集中是否包含不为空的action这个属性，如果该属性为空或不存在则表示为控制器级的过滤器。</para>
	///		<example>
	///		<![CDATA[
	///		<extension path="/Workspace/Web/Filters">
	///			<object name="global-filter"
	///			        type="AAA.BBB.XXXFilter, assemblyName"
	///			        method="POST|GET|DELETE|PUT" />
	///		</extension>
	///		<extension path="/Workspace/Web/Filters/Api">
	///			<object name="area-filter"
	///			        type="AAA.BBB.XXXFilter, assemblyName"
	///			        method="POST|GET|DELETE|PUT" />
	///		</extension>
	///		<extension path="/Workspace/Web/Filters/Api/Security">
	///			<object name="area-filter"
	///			        type="AAA.BBB.XXXFilter, assemblyName"
	///			        method="POST|GET|DELETE|PUT" />
	///		</extension>
	///		<extension path="/Workspace/Web/Filters/Api/Security/User">
	///			<object name="controller-action-filter"
	///			        type="AAA.BBB.XXXFilter, assemblyName"
	///			        action="ActionName1, ActionName2..."
	///			        method="POST|GET|DELETE|PUT" />
	///		</extension>
	///		]]>
	///		</example>
	/// </remarks>
	public abstract class PluginFilterProviderBase<T> where T : class
	{
		#region 枚举定义
		protected enum FilterScope
		{
			Global,
			Controller,
			Action,
		}
		#endregion

		#region 成员字段
		private string _path;
		private Zongsoft.Plugins.PluginContext _pluginContext;
		#endregion

		#region 构造函数
		protected PluginFilterProviderBase(Zongsoft.Plugins.PluginContext pluginContext, string path = null)
		{
			if(pluginContext == null)
				throw new ArgumentNullException(nameof(pluginContext));

			_pluginContext = pluginContext;
			_path = string.IsNullOrWhiteSpace(path) ? "/Workspace/Web/Filters" : path.Trim();
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置过滤器集合的插件路径。
		/// </summary>
		public string Path
		{
			get
			{
				return _path;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_path = value;
			}
		}
		#endregion

		#region 保护方法
		protected IEnumerable<T> GetFilters(string area, string controllerName, string actionName, string method)
		{
			area = area.Trim('/', ' ', '\t', '\r', '\n');
			controllerName = controllerName.Trim('/', ' ', '\t', '\r', '\n');

			foreach(var filter in this.GetGlobalFilters(area, method))
				yield return filter;

			var node = _pluginContext.PluginTree.Find(_path, area, controllerName);

			if(node == null)
				yield break;

			foreach(var child in node.Children)
			{
				if(child.NodeType == Plugins.PluginTreeNodeType.Empty || !this.ContainsHttpMethod(child.Properties.GetRawValue("method"), method))
					continue;

				T filter = null;

				if(child.Properties.TryGetValue("action", out var value) && value is string)
				{
					if(string.Equals(actionName, (string)value, StringComparison.OrdinalIgnoreCase))
						filter = this.CreateFilter(child, FilterScope.Action);
				}
				else
				{
					filter = this.CreateFilter(child, FilterScope.Controller);
				}

				if(filter != null)
					yield return filter;
			}
		}
		#endregion

		#region 抽象方法
		protected abstract T CreateFilter(Zongsoft.Plugins.PluginTreeNode node, FilterScope scope);
		#endregion

		#region 私有方法
		private IEnumerable<T> GetGlobalFilters(string area, string method)
		{
			var node = _pluginContext.PluginTree.Find(_path);

			foreach(var filter in this.GetGlobalFilters(node, method))
				yield return filter;

			if(string.IsNullOrWhiteSpace(area))
				yield break;

			var parts = area.Split('/');

			foreach(var part in parts)
			{
				if(string.IsNullOrWhiteSpace(part))
					continue;

				node = _pluginContext.PluginTree.Find(_path, part);

				foreach(var filter in this.GetGlobalFilters(node, method))
					yield return filter;
			}
		}

		private IEnumerable<T> GetGlobalFilters(Zongsoft.Plugins.PluginTreeNode node, string method)
		{
			if(node == null)
				yield break;

			foreach(var child in node.Children)
			{
				if(child.NodeType == Plugins.PluginTreeNodeType.Empty)
					continue;

				if(this.ContainsHttpMethod(child.Properties.GetRawValue("method"), method))
				{
					var filter = this.CreateFilter(child, FilterScope.Global);

					if(filter != null)
						yield return filter;
				}
			}
		}

		private bool ContainsHttpMethod(string testMethod, string requestMethod)
		{
			if(string.IsNullOrWhiteSpace(testMethod) || testMethod.Trim() == "*")
				return true;

			var parts = testMethod.Split(',');

			return parts.Any(part => string.Equals(part.Trim(), requestMethod.Trim(), StringComparison.OrdinalIgnoreCase));
		}
		#endregion
	}
}
