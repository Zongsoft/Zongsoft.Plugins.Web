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
using System.Web;
using System.Web.UI;

using Zongsoft.Plugins.Builders;

namespace Zongsoft.Plugins.Web.Builders
{
	[BuilderBehavior(typeof(Sitemap))]
	public class SitemapBuilder : ControlBuilder
	{
		#region 重写方法
		public override object Build(BuilderContext context)
		{
			var path = context.Builtin.Properties.GetRawValue("path");
			PluginTreeNode node = null;

			if(string.IsNullOrWhiteSpace(path))
				node = context.PluginTree.Find(context.Builtin.FullPath);
			else
				node = context.PluginTree.Find(path);

			return new Sitemap(node)
			{
				ID = context.Builtin.Name,
				CssClass = context.Builtin.Properties.GetValue<string>("cssClass"),
			};
		}
		#endregion

		#region 站点控件
		/// <summary>
		/// 该控件根据指定的插件节点生成特定结构的站点地图。
		/// </summary>
		/// <remarks>
		/// 该控件生成的站点地图的HTML结构大致如下：
		///		<code>
		///		<![CDATA[
		///		<dl>
		///			<dt>#1<a href="{url}" alt="{toolTip}">{title}</a></dt>
		///			<dd>#1{description}</dd>
		///			<dt>#2<a href="{url}" alt="{toolTip}">{title}</a></dt>
		///			<dd>#2{description}</dd>
		///			<dt>#3<a href="{url}" alt="{toolTip}">{title}</a></dt>
		///			<dd>#3{description}</dd>
		///		</dl>
		///		]]>
		///		</code>
		/// </remarks>
		public class Sitemap : System.Web.UI.Control
		{
			#region 成员变量
			private Zongsoft.Plugins.PluginTreeNode _sitemapNode;
			private string _cssClass;
			#endregion

			#region 构造函数
			internal Sitemap(Zongsoft.Plugins.PluginTreeNode sitemapNode)
			{
				if(sitemapNode == null)
					throw new ArgumentNullException("sitemapNode");

				_sitemapNode = sitemapNode;
			}
			#endregion

			#region 公共属性
			public string CssClass
			{
				get
				{
					return _cssClass;
				}
				set
				{
					_cssClass = value ?? string.Empty;
				}
			}
			#endregion

			#region 重写方法
			public override void RenderControl(HtmlTextWriter writer)
			{
				if(!this.Visible)
					return;

				string url = Zongsoft.Web.VirtualPathHelper.GetVirtualPath(HttpContext.Current.Request.RequestContext.RouteData);
				var currentNode = this.GetCurrentNode(_sitemapNode, url);

				if(currentNode == null)
					return;

				var nodes = this.GetCurrentNodePath(currentNode);

				if(nodes == null)
					return;

				if(!string.IsNullOrWhiteSpace(_cssClass))
					writer.AddAttribute(HtmlTextWriterAttribute.Class, _cssClass);

				writer.RenderBeginTag(HtmlTextWriterTag.Dl);
				this.RenderSitemapNodes(writer, nodes);
				writer.RenderEndTag();
			}
			#endregion

			#region 私有方法
			private Zongsoft.Plugins.PluginTreeNode GetCurrentNode(Zongsoft.Plugins.PluginTreeNode node, string url)
			{
				if(node == null || string.IsNullOrWhiteSpace(url))
					return null;

				foreach(var child in node.Children)
				{
					var foundNode = this.GetCurrentNode(child, url);
					if(foundNode != null)
						return foundNode;
				}

				var treeNode = node.UnwrapValue(ObtainMode.Auto, new BuilderSettings(this)) as Zongsoft.Web.Controls.TreeViewNode;

				if(treeNode != null)
				{
					if((!string.IsNullOrWhiteSpace(treeNode.NavigateUrl)) && url.StartsWith(treeNode.NavigateUrl, StringComparison.OrdinalIgnoreCase))
						return node;
				}

				return null;
			}

			private Zongsoft.Plugins.PluginTreeNode[] GetCurrentNodePath(Zongsoft.Plugins.PluginTreeNode currentNode)
			{
				if(currentNode == null)
					return null;

				var stack = new Stack<Zongsoft.Plugins.PluginTreeNode>();

				while(currentNode != null && currentNode != _sitemapNode)
				{
					stack.Push(currentNode);
					currentNode = currentNode.Parent;
				}

				//最后再将主页压入堆栈
				stack.Push(_sitemapNode);

				return stack.ToArray();
			}

			private void RenderSitemapNodes(HtmlTextWriter writer, IEnumerable<Zongsoft.Plugins.PluginTreeNode> nodes)
			{
				if(nodes == null)
					return;

				var last = nodes.LastOrDefault();

				foreach(var node in nodes)
				{
					var treeNode = node.UnwrapValue(ObtainMode.Auto, new BuilderSettings(this)) as Zongsoft.Web.Controls.TreeViewNode;

					if(treeNode != null)
					{
						//生成<dt>开始元素
						writer.RenderBeginTag(HtmlTextWriterTag.Dt);

						if(node == last)
						{
							writer.WriteEncodedText(treeNode.Text);
						}
						else
						{
							writer.AddAttribute(HtmlTextWriterAttribute.Href, string.IsNullOrWhiteSpace(treeNode.NavigateUrl) ? "#" : treeNode.NavigateUrl);
							writer.AddAttribute(HtmlTextWriterAttribute.Alt, treeNode.ToolTip);
							writer.RenderBeginTag(HtmlTextWriterTag.A);
							writer.WriteEncodedText(treeNode.Text);
							writer.RenderEndTag();
						}

						//生成</dt>结束元素
						writer.RenderEndTag();

						//处理描述元素
						if(!string.IsNullOrWhiteSpace(treeNode.Description))
						{
							writer.RenderBeginTag(HtmlTextWriterTag.Dd);
							writer.WriteEncodedText(treeNode.Description);
							writer.RenderEndTag();
						}
					}
				}
			}
			#endregion
		}
		#endregion
	}
}
