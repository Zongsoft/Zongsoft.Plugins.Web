﻿/*
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
using System.IO;
using System.Web;

namespace Zongsoft.Plugins.Web
{
	public class ApplicationContext : Zongsoft.Plugins.PluginApplicationContext
	{
		#region 单例字段
		public new static readonly ApplicationContext Current = new ApplicationContext();
		#endregion

		#region 成员字段
		private string _applicationDirectory;
		private Zongsoft.Options.Configuration.OptionConfiguration _configuration;
		#endregion

		#region 构造函数
		private ApplicationContext() : base("Zongsoft.Plugins.Web")
		{
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前Web应用程序的上下文对象。
		/// </summary>
		public HttpContext HttpContext
		{
			get
			{
				return HttpContext.Current;
			}
		}
		#endregion

		#region 重写方法
		public override string ApplicationDirectory
		{
			get
			{
				if(string.IsNullOrEmpty(_applicationDirectory))
					_applicationDirectory = HttpContext.Current.Server.MapPath("~");

				return _applicationDirectory;
			}
		}

		public override Zongsoft.Options.Configuration.OptionConfiguration Configuration
		{
			get
			{
				if(_configuration == null)
				{
					string filePaht = Path.Combine(this.ApplicationDirectory, "Web.option");

					if(File.Exists(filePaht))
						_configuration = Zongsoft.Options.Configuration.OptionConfiguration.Load(filePaht);
					else
						_configuration = new Options.Configuration.OptionConfiguration(filePaht);
				}

				return _configuration;
			}
		}

		public override System.Security.Principal.IPrincipal Principal
		{
			get
			{
				return HttpContext.Current.User;
			}
		}

		protected override IWorkbenchBase CreateWorkbench(string[] args)
		{
			PluginTreeNode node = this.PluginContext.PluginTree.Find(this.PluginContext.Settings.WorkbenchPath);

			if(node != null && node.NodeType == PluginTreeNodeType.Builtin)
				return base.CreateWorkbench(args);

			return new Workbench(this);
		}
		#endregion
	}
}
