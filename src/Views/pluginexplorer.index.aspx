<%@ Page Title="插件管理器" Language="C#" MasterPageFile="~/Views/Site.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="BodyPlaceHolder" runat="server">
<form action="/PluginExplorer/Index" method="post">
	<div id="layoutContainer">
	<zs:Widget ID="InquiryWidget" Title="插件树" style="float:left" runat="server">
		<ContentTemplate>
			<zs:TreeView ID="PluginTree" DataSource="${ViewData[PluginTree].RootNode}" SelectedPath="${ViewData[Path]}" Width="540px" Height="320px" ScrollbarMode="Vertical" runat="server">
				<RootNodeBinding KeyPropertyName="Name" Text="根节点" Url="/PluginExplorer?path=${FullPath}" ChildrenPropertyName="Children" />
				<NodeBinding KeyPropertyName="Name" Text="Name" Url="/PluginExplorer?path=${FullPath}" ChildrenPropertyName="Children" />
			</zs:TreeView>
		</ContentTemplate>
	</zs:Widget>

	<zs:Widget ID="ResultWidget" Title="元素属性 [${Model.NodeType}]" runat="server">
		<ContentTemplate>
			<zs:LayoutPanel LayoutColumns="2" LayoutMode="Table" runat="server">
				<zs:TextBox Name="BuilderName" Label="构建器名" Text="${Model.Value.BuilderName}" ReadOnly="True" runat="server" />
				<zs:TextBox Name="Builtin.Name" Label="构件名称" Text="${Model.Value.Name}" ReadOnly="True" runat="server" />
				<zs:TextBox Name="Builtin.Position" Label="构件位置" Text="${Model.Value.Position}" ReadOnly="True" runat="server" />
				<zs:TextBox Name="Builtin.IsBuilded" Label="是否创建" Text="${Model.Value.IsBuilded}" ReadOnly="True" runat="server" />

				<zs:LayoutPanelCell ColSpan="2" runat="server">
					<zs:TextBox Name="FullPath" Label="节点路径" Text="${Model.FullPath}" ReadOnly="True" Width="100%" runat="server" />
				</zs:LayoutPanelCell>

				<zs:TextBox Name="Plugin.Name" Label="插件名称" Text="${Model.Plugin.Name}" ReadOnly="True" runat="server" />
				<zs:TextBox Name="Plugin.Manifest.Version" Label="插件版本" Text="${Model.Plugin.Manifest.Version}" ReadOnly="True" runat="server" />

				<zs:LayoutPanelCell ColSpan="2" runat="server">
					<zs:TextBox Name="Plugin.FileName" Label="插件文件" Text="${Model.Plugin.FileName}" ReadOnly="True" runat="server" />
				</zs:LayoutPanelCell>
				<zs:LayoutPanelCell ColSpan="2" runat="server">
					<zs:TextBox Name="Plugin.Manifest.Title" Label="插件标题" Text="${Model.Plugin.Manifest.Title}" ReadOnly="True" runat="server" />
				</zs:LayoutPanelCell>
				<zs:LayoutPanelCell ColSpan="2" runat="server">
					<zs:TextBox Name="Plugin.Manifest.Author" Label="插件作者" Text="${Model.Plugin.Manifest.Author}" ReadOnly="True" runat="server" />
				</zs:LayoutPanelCell>
			</zs:LayoutPanel>

			<zs:TabStrip ID="tabStrip" runat="server">
				<Panels>
					<zs:TabStripPanel Name="Properties" Title="构件属性" Description="关于构件的自定义属性集。">
						<Content>
							<zs:ListView ID="BuiltinProperties"
										DataSource="${Model.Value.Properties}"
										ItemTemplateText="[${Index}] ${DataItem} -> ${DataSource[0]}"
										runat="server">
								<EmptyTemplate>
									<p>无自定属性。</p>
								</EmptyTemplate>
							</zs:ListView>
						</Content>
					</zs:TabStripPanel>

					<zs:TabStripPanel Name="Target" Title="目标对象" Description="关于当前插件路径对应的目标对象。">
						<Content>
							<zs:ListView ID="TargetProperties"
										DataSource="${ViewData[Target-Properties]}"
										ItemTemplateText="[${Index}] ${DataItem} -> ${DataSource[0]}"
										runat="server">
								<EmptyTemplate>
									<p>无目标对象。</p>
								</EmptyTemplate>
							</zs:ListView>
						</Content>
					</zs:TabStripPanel>
				</Panels>
			</zs:TabStrip>
		</ContentTemplate>
	</zs:Widget>
	</div>
</form>
</asp:Content>
