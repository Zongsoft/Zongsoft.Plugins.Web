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
using System.IO;
using System.Web.Hosting;
using System.Web.Routing;
using System.Text.RegularExpressions;

namespace Zongsoft.Web
{
	internal static class VirtualPathHelper
	{
		#region 私有变量
		private static readonly Regex _urlRegex = new Regex(@"{(?'var'[^{}]+)}", (RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase));
		private static readonly Regex _areaRegex = new Regex(@"\s*(?<area>.+)/{controller}.*", (RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled));
		#endregion

		#region 公共方法
		/// <summary>
		/// 将指定的物理路径转换成虚拟路径。
		/// </summary>
		/// <param name="physicalPath">指定的物理路径。</param>
		/// <returns>返回物理路径对应的虚拟路径。</returns>
		/// <remarks>
		///		<para>如果<paramref name="physicalPath"/>参数对应的物理路径不位于当前Web应用程序的根目录下，则将指定的物理路径简单转换成相对的虚拟路径。</para>
		/// </remarks>
		public static string ToVirtualPath(string physicalPath)
		{
			if(string.IsNullOrEmpty(physicalPath))
				throw new ArgumentNullException("physicalPath");

			string path;

			if(physicalPath.StartsWith(HostingEnvironment.ApplicationPhysicalPath, StringComparison.OrdinalIgnoreCase))
				path = physicalPath.Substring(HostingEnvironment.ApplicationPhysicalPath.Length);
			else
				path = physicalPath;

			return "~/" + path.Replace(Path.DirectorySeparatorChar, '/');
		}

		/// <summary>
		/// 根据当前<see cref="System.Web.Routing.RouteData"/>路由数据及其对应的路由Url，解析得到对应的虚拟路径。
		/// </summary>
		/// <param name="routeData">待解析的路由数据。</param>
		/// <returns>解析后的虚拟路径。</returns>
		public static string GetVirtualPath(RouteData routeData)
		{
			if(routeData == null)
				return string.Empty;

			Route route = routeData.Route as Route;

			if(route == null)
				return string.Empty;

			string url = _urlRegex.Replace(route.Url, match =>
			{
				string key = match.Groups["var"].Value;
				object value;

				if(routeData.Values.TryGetValue(key, out value) && value != null)
					return value == System.Web.Mvc.UrlParameter.Optional ? string.Empty : value.ToString();
				else
					return string.Empty;
			});

			if(url[0] != '/')
				return '/' + url.TrimEnd('/');
			else
				return url.TrimEnd('/');
		}

		public static string GetArea(RouteData routeData)
		{
			if(routeData == null)
				throw new ArgumentNullException("routeData");

			object areaValue;

			if(routeData.Values.TryGetValue("area", out areaValue))
				return areaValue.ToString();

			Route route = routeData.Route as Route;

			if(route != null)
			{
				if(route.DataTokens.TryGetValue("area", out areaValue))
					return areaValue.ToString();
				else
					return GetArea(route.Url);
			}

			return null;
		}

		public static string GetArea(string url)
		{
			string area = string.Empty;
			Match match = _areaRegex.Match(url);

			if(match.Success && match.Groups["area"].Success)
			{
				if(match.Groups["area"].Captures.Count > 1)
				{
					for(int i = 0; i < match.Groups["area"].Captures.Count; i++)
						area = area + match.Groups["area"].Captures[i].Value;
				}
				else
				{
					area = match.Groups["area"].Value;
				}
			}

			return area;
		}
		#endregion
	}
}
