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

namespace Zongsoft.Plugins.Web.Builders
{
	public class RouteBuilder : Zongsoft.Plugins.Builders.BuilderBase
	{
		#region 重写方法
		public override object Build(Zongsoft.Plugins.Builders.BuilderContext context)
		{
			var builtin = context.Builtin;

			var token = new Zongsoft.Web.Routing.RouteToken(
				builtin.Name,
				builtin.Properties.GetRawValue("kind"),
				builtin.Properties.GetRawValue("url"),
				builtin.Properties.GetValue<object>("handler", null));

			this.ResolveValues(token.Defaults, builtin.Properties.GetValue<string>("defaults"));
			this.ResolveValues(token.Constraints, builtin.Properties.GetValue<string>("constraints"));
			this.ResolveValues(token.States, builtin.Properties.GetValue<string>("states"));

			if(!token.States.ContainsKey("area"))
			{
				object area = null;

				if(token.Constraints.TryGetValue("area", out area))
					token.States.Add("area", area);
				else
					token.States.Add("area", Zongsoft.Web.VirtualPathHelper.GetArea(token.Url));
			}

			return token;
		}
		#endregion

		#region 私有方法
		private void ResolveValues(IDictionary<string, object> dictionary, string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return;

			var parts = text.Split(',');

			foreach(string part in parts)
			{
				int index = part.IndexOf('=');

				if(index > 0)
				{
					string key = part.Substring(0, index).Trim();

					if(!string.IsNullOrEmpty(key))
					{
						string valueText = part.Substring(index + 1);

						if(string.IsNullOrWhiteSpace(valueText))
							dictionary.Add(key, null);
						else
							dictionary.Add(key, valueText);
					}
				}
			}
		}
		#endregion
	}
}
