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
using System.Collections.Generic;
using System.Web.Mvc;

using Zongsoft.Plugins;

namespace Zongsoft.Web
{
	public class PluginFilterProvider : PluginFilterProviderBase<Filter>, IFilterProvider
	{
		#region 构造函数
		public PluginFilterProvider(PluginContext pluginContext) : base(pluginContext)
		{
		}
		#endregion

		#region 公共方法
		public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
		{
			return this.GetFilters(
				(string)controllerContext.RouteData.DataTokens["area"],
				actionDescriptor.ControllerDescriptor.ControllerName,
				actionDescriptor.ActionName,
				controllerContext.HttpContext.Request.HttpMethod);
		}
		#endregion

		#region 重写方法
		protected override Filter CreateFilter(PluginTreeNode node, FilterScope scope)
		{
			var value = node.UnwrapValue(ObtainMode.Auto, null);

			if(value is Filter)
				return (Filter)value;

			var filter = new Filter(value, this.GetFilterScope(scope), null);
			node.Tree.Mount(node, filter);
			return filter;
		}
		#endregion

		#region 私有方法
		private System.Web.Mvc.FilterScope GetFilterScope(FilterScope scope)
		{
			switch(scope)
			{
				case FilterScope.Global:
					return System.Web.Mvc.FilterScope.Global;
				case FilterScope.Controller:
					return System.Web.Mvc.FilterScope.Controller;
				case FilterScope.Action:
					return System.Web.Mvc.FilterScope.Action;
			}

			throw new InvalidOperationException("Invalid value of the filter scope.");
		}
		#endregion
	}
}
