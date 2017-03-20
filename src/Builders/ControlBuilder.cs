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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Zongsoft.Plugins;
using Zongsoft.Plugins.Builders;

namespace Zongsoft.Plugins.Web.Builders
{
	[BuilderBehavior(typeof(Control))]
	public class ControlBuilder : BuilderBase
	{
		#region 构造函数
		/// <summary>
		/// 创建控件构造器。
		/// </summary>
		/// <remarks>
		///		<para>在构建目标对象时忽略“place”属性，该属性由视图引擎处理。</para>
		/// </remarks>
		public ControlBuilder() : base(new string[] { "place" })
		{
		}
		#endregion

		#region 重写方法
		protected override void OnBuilt(BuilderContext context)
		{
			Control itemControl = context.Result as Control;

			if(itemControl == null)
				return;

			itemControl.ID = context.Builtin.Name;
			Control ownerControl = context.Owner as Control ?? context.Parameter as Control;

			if(ownerControl == null)
				return;

			string index = context.Builtin.Properties.GetValue<string>("index");

			if(!string.IsNullOrWhiteSpace(index))
			{
				for(int i = 0; i < ownerControl.Controls.Count; i++)
				{
					if(string.Equals(ownerControl.Controls[i].ID, index, StringComparison.OrdinalIgnoreCase))
					{
						ownerControl.Controls.AddAt(i, itemControl);
						return;
					}
				}
			}

			ownerControl.Controls.Add(itemControl);
		}
		#endregion
	}
}
