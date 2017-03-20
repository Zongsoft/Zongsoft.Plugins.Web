/*
 * Authors:
 *   谢林青(Alphaair Xie) <alphaair@163.com>
 *
 * Copyright (C) 2011-2014 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Web.Mvc;
using System.Web.Http.Controllers;

namespace Zongsoft.Web.Http
{
	public class PluginHttpActionSelector : ApiControllerActionSelector
	{
		public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
		{
			var action = controllerContext.RouteData.Values["action"];

			if(action == null || action == UrlParameter.Optional)
				controllerContext.RouteData.Values["action"] = controllerContext.Request.Method.Method;

			//调用基类同名方法
			return base.SelectAction(controllerContext);
		}
	}
}
