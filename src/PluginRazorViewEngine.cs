/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Web;
using System.Web.Mvc;

namespace Zongsoft.Web
{
	public class PluginRazorViewEngine : PluginViewEngine
	{
		#region 私有常量
		private readonly string[] FileExtensions = new[]
		{
			"cshtml",
			"vbhtml",
		};
		#endregion

		#region 构造函数
		public PluginRazorViewEngine(Zongsoft.Plugins.PluginContext pluginContext) : base(pluginContext)
		{
			this.ViewExtensions = FileExtensions;
			this.PartialViewExtensions = FileExtensions;
			this.MasterExtensions = FileExtensions;
		}
		#endregion

		#region 创建视图
		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
		{
			return new RazorView(controllerContext, viewPath, layoutPath: masterPath, runViewStartPages: true, viewStartFileExtensions: FileExtensions);
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
		{
			return new RazorView(controllerContext, partialPath,
				layoutPath: null, runViewStartPages: false, viewStartFileExtensions: FileExtensions);
		}
		#endregion
	}
}

