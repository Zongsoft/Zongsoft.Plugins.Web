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

using Zongsoft.Plugins.Builders;

namespace Zongsoft.Plugins.Web.Builders
{
	[BuilderBehavior(typeof(Zongsoft.Web.Controls.TreeViewNode))]
	public class TreeViewNodeBuilder : BuilderBase
	{
		#region 重写方法
		public override object Build(BuilderContext context)
		{
			Builtin builtin = context.Builtin;

			var node = new Zongsoft.Web.Controls.TreeViewNode(builtin.Name, builtin.Properties.GetValue<string>("text"))
			{
				Icon = builtin.Properties.GetValue<string>("icon"),
				Text = builtin.Properties.GetValue<string>("text"),
				ToolTip = builtin.Properties.GetValue<string>("tooltip"),
				Description = builtin.Properties.GetValue<string>("description"),
				NavigateUrl = builtin.Properties.GetValue<string>("url") ?? string.Empty,
				NavigateCssClass = builtin.Properties.GetValue<string>("navigateCssClass"),
				CssClass = builtin.Properties.GetValue<string>("cssClass"),
				ListCssClass = builtin.Properties.GetValue<string>("listCssClass"),
				Selected = builtin.Properties.GetValue<bool>("selected", false),
				Visible = builtin.Properties.GetValue<bool>("visible", true),
			};

			node.Image.CssClass = builtin.Properties.GetValue<string>("image-cssClass");
			node.Image.Dimension = builtin.Properties.GetValue("image-dimension", Zongsoft.Web.Controls.Dimension.None);
			node.Image.ImageUrl = builtin.Properties.GetValue<string>("image-url");
			node.Image.NavigateUrl = builtin.Properties.GetValue<string>("image-navigateUrl");
			node.Image.Placeholder = builtin.Properties.GetValue<string>("image-placeholder");

			//返回构建的目标对象
			return node;
		}

		protected override void OnBuildComplete(BuilderContext context)
		{
			if(context.Owner == null)
				return;

			var node = context.Result as Zongsoft.Web.Controls.TreeViewNode;

			if(node == null)
				return;

			//根据所有者对象的类型，将当前目标对象添加到其子项列表中
			if(context.Owner is Zongsoft.Web.Controls.TreeViewNode)
			{
				((Zongsoft.Web.Controls.TreeViewNode)context.Owner).Nodes.Add(node);
			}
			else if(context.Owner is Zongsoft.Web.Controls.TreeView)
			{
				((Zongsoft.Web.Controls.TreeView)context.Owner).Nodes.Add(node);
			}
		}
		#endregion
	}
}
