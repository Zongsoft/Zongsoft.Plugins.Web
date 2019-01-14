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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
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
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;

using Zongsoft.Plugins;

namespace Zongsoft.Web.Http
{
	public class PluginHttpFilterProvider : PluginFilterProviderBase<FilterInfo>, System.Web.Http.Filters.IFilterProvider
	{
		#region 构造函数
		public PluginHttpFilterProvider(PluginContext pluginContext) : base(pluginContext, "/Workspace/Web/Api/Filters")
		{
		}
		#endregion

		#region 公共方法
		public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
		{
			object area, method;

			actionDescriptor.ControllerDescriptor.Properties.TryGetValue("route.area", out area);
			actionDescriptor.ControllerDescriptor.Properties.TryGetValue("http.method", out method);

			return this.GetFilters(
				area as string,
				actionDescriptor.ControllerDescriptor.ControllerName,
				actionDescriptor.ActionName,
				method as string);
		}
		#endregion

		#region 重写方法
		protected override FilterInfo CreateFilter(PluginTreeNode node, FilterScope scope)
		{
			var value = node.UnwrapValue(ObtainMode.Auto, null);

			if(value is FilterInfo)
				return (FilterInfo)value;

			if(value is IFilter)
			{
				var filterInfo = new FilterInfo((IFilter)value, this.GetFilterScope(scope));
				node.Tree.Mount(node, filterInfo);
				return filterInfo;
			}

			return null;
		}
		#endregion

		#region 私有方法
		private System.Web.Http.Filters.FilterScope GetFilterScope(FilterScope scope)
		{
			switch(scope)
			{
				case FilterScope.Global:
					return System.Web.Http.Filters.FilterScope.Global;
				case FilterScope.Controller:
					return System.Web.Http.Filters.FilterScope.Controller;
				case FilterScope.Action:
					return System.Web.Http.Filters.FilterScope.Action;
			}

			throw new InvalidOperationException("Invalid value of the filter scope.");
		}
		#endregion
	}
}
