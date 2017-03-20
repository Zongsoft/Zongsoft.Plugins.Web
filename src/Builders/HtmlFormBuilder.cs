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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

using Zongsoft.Plugins;
using Zongsoft.Plugins.Builders;

namespace Zongsoft.Plugins.Web.Builders
{
	[BuilderBehavior(typeof(HtmlForm))]
	public class HtmlFormBuilder : ControlBuilder
	{
		#region 重写方法
		public override object Build(BuilderContext context)
		{
			return new HtmlForm()
			{
				Action = context.Builtin.Properties.GetValue<string>("action"),
				Method = context.Builtin.Properties.GetValue<string>("method"),
			};
		}
		#endregion

		#region 嵌套子类
		public class HtmlForm : Zongsoft.Web.Controls.DataBoundControl
		{
			#region 公共属性
			[Bindable(true)]
			[Zongsoft.Web.Controls.PropertyMetadata(PropertyRender = "Zongsoft.Web.Controls.UrlPropertyRender.Default, Zongsoft.Web")]
			public string Action
			{
				get
				{
					return this.GetPropertyValue(() => this.Action);
				}
				set
				{
					this.SetPropertyValue(() => this.Action, value);
				}
			}

			[Bindable(true)]
			[DefaultValue("POST")]
			public string Method
			{
				get
				{
					return this.GetPropertyValue(() => this.Method);
				}
				set
				{
					this.SetPropertyValue(() => this.Method, value);
				}
			}
			#endregion

			#region 重写方法
			protected override void Render(HtmlTextWriter writer)
			{
				//生成所有元素标签
				this.AddAttributes(writer);

				writer.RenderBeginTag(HtmlTextWriterTag.Form);

				//生成表单内容的所有子控件
				this.RenderChildren(writer);

				writer.RenderEndTag();
			}
			#endregion
		}
		#endregion
	}
}
