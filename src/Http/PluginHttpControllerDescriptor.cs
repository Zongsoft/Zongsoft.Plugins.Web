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
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

using Zongsoft.Plugins;

namespace Zongsoft.Web.Http
{
	public class PluginHttpControllerDescriptor : System.Web.Http.Controllers.HttpControllerDescriptor
	{
		#region 成员字段
		private PluginTreeNode _controllerNode;
		#endregion

		#region 构造函数
		public PluginHttpControllerDescriptor(HttpConfiguration configuration, PluginTreeNode controllerNode)
		{
			if(configuration == null)
				throw new ArgumentNullException("configuration");

			if(controllerNode == null)
				throw new ArgumentNullException("controllerNode");

			_controllerNode = controllerNode;
			this.Configuration = configuration;
			this.ControllerName = controllerNode.Name;

			switch(controllerNode.NodeType)
			{
				case PluginTreeNodeType.Builtin:
					this.ControllerType = ((Builtin)controllerNode.Value).BuiltinType.Type;
					break;
				case PluginTreeNodeType.Custom:
					if(controllerNode.Value != null)
						this.ControllerType = controllerNode.Value.GetType();
					break;
			}
		}
		#endregion

		#region 公共属性
		public PluginTreeNode ControllerNode
		{
			get
			{
				return _controllerNode;
			}
		}
		#endregion

		#region 重写方法
		public override IHttpController CreateController(System.Net.Http.HttpRequestMessage request)
		{
			return _controllerNode.UnwrapValue<IHttpController>(ObtainMode.Alway, request, null);
		}
		#endregion
	}
}
