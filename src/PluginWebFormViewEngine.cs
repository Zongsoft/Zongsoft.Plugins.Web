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

namespace Zongsoft.Web
{
	public class PluginWebFormViewEngine : PluginViewEngine
	{
		#region 构造函数
		public PluginWebFormViewEngine(Zongsoft.Plugins.PluginContext pluginContext) : base(pluginContext)
		{
			this.ViewExtensions = new string[] { "aspx", "html", "htm" };
			this.PartialViewExtensions = new string[] { "ascx" };
			this.MasterExtensions = new string[] { "master" };
		}
		#endregion

		#region 创建视图
		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
		{
			return new PluginWebFormView(this.PluginContext, viewPath, masterPath);
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
		{
			return new PluginWebFormView(this.PluginContext, partialPath, null);
		}
		#endregion
	}
}
