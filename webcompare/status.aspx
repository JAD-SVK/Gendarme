<%@ Page Language="C#" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="GuiCompare" %>
<!--

TODO:

  Add Error messages generated by the compare process.

-->
<script runat="server" language="c#">

public CompareContext compare_context;

const string ImageMissing = "<img src='sm.gif' border=0 align=absmiddle title='Missing'>";
const string ImageExtra   = "<img src='sx.gif' border=0 align=absmiddle title='Extra'>";
const string ImageOk      = "<img src='sc.gif' border=0 align=absmiddle>";
const string ImageError   = "<img src='se.gif' border=0 align=absmiddle title='throw NotImplementedException'>";
const string ImageWarning = "<img src='mn.png' border=0 align=absmiddle title='warning'>";

static string ImageTodo (ComparisonNode cn)
{
	return String.Format ("<img src='st.gif' border=0 align=absmiddle title='{0}'>", GetTodo (cn));
}

static string Get (int count, string kind, string caption)
{
	if (count == 0)
		return "";
	
	return String.Format ("<div class='report' title='{0} {2}'><div class='icons suffix {1}'></div>{0}</div>", count, kind, caption);
}
	  
static string GetStatus (ComparisonNode n)
{
	string status = 
		Get (n.Missing, "missing", "missing members") +
		Get (n.Extra, "extra", "extra members") +
		Get (n.Warning, "warning", "warnings") +
		Get (n.Todo, "todo", "items with notes") +
		Get (n.Niex, "niex", "members that throw NotImplementedException");

	if (status != "")
		return n.Name + status;

	return n.Name;
}

public void ShowPleaseWait ()
{
	waitdiv.Visible = true;
	
	ClientScript.RegisterStartupScript(
	    this.GetType(),
	    "postpleasewait",
	    ClientScript.GetPostBackEventReference(new PostBackOptions (waitdiv)) + ";",
	    true);
}

global_asax.CompareParameters compare_param;

public void Page_Load ()
{
	var cp = new global_asax.CompareParameters (Page.Request.QueryString);
	compare_param = cp;
	if (!global_asax.CompareParameters.InCache (cp) && !IsPostBack){
		ShowPleaseWait ();
		return;
	}

	compare_context = cp.GetCompareContext ();
	var n = compare_context.Comparison;

	waitdiv.Visible = false;

	//TreeNode tn = new TreeNode ("<img src='sm.gif' border=0 align=absmiddle>" + n.name);
	//TreeNode tn = new TreeNode (n.name);
	//TreeNode tn = new TreeNode ("<div class='ok'></div>" + n.name);

	TreeNode tn = new TreeNode (GetStatus (n), n.Name);
	tn.SelectAction = TreeNodeSelectAction.None;
	tn.PopulateOnDemand = true;
	tree.Nodes.Add (tn);

	var diff = DateTime.UtcNow - global_asax.CompareParameters.GetAssemblyTime (cp);
	string t;
	if (diff.Days > 1)
		t = String.Format ("{0} days", diff.Days);
	else if (diff.Hours > 2)
		t = String.Format ("{0} hours", diff.Hours);
	else if (diff.Minutes > 2)
	        t = String.Format ("{0} minutes", diff.Minutes);
	else 
	        t = String.Format ("{0} seconds", diff.Seconds);

	time_label.Text = String.Format ("Assembly last updated: {0} ago", t);
	activediv.Visible = true;
}

static string GetTodo (ComparisonNode cn)
{
	StringBuilder sb = new StringBuilder ();
	foreach (string s in cn.Todos){
		string clean = s.Substring (20, s.Length-22);
		if (clean == "")
			sb.Append ("Flagged with TODO");
		else {
			sb.Append ("Comment: ");
			sb.Append (clean);
			sb.Append ("<br>");
		}
	}
	return sb.ToString ();
}

static string GetMessages (ComparisonNode cn)
{
	StringBuilder sb = new StringBuilder ();
	foreach (string s in cn.Messages){
		sb.Append (s);
		sb.Append ("<br>");
	}
	return sb.ToString ();
}

static string ImagesFromCounts (ComparisonNode cn)
{
	int x = (cn.Todo != 0 ? 2 : 0) | (cn.Warning != 0 ? 1 : 0);
	switch (x){
        case 0:
       		return "";
	case 1:
		return ImageWarning;
	case 2:
	        return ImageTodo (cn);
	case 4:
	        return ImageTodo (cn) + ImageWarning;
	}
	return "";
}

static string MemberStatus (ComparisonNode cn)
{
	if (cn.Niex != 0)
		cn.Status = ComparisonStatus.Error;

	string counts = ImagesFromCounts (cn);

	switch (cn.Status) {
	case ComparisonStatus.None:
	        return counts == "" ? ImageOk : ImageOk + counts;
		
	case ComparisonStatus.Missing:
		return ImageMissing;
		
	case ComparisonStatus.Extra:
		return counts == "" ? ImageExtra : ImageOk + counts;
		
	case ComparisonStatus.Error:
	        return counts == "" ? ImageError : ImageError + counts;

	default:
		return "Unknown status: " + cn.Status;
	}
}

// {0} = MemberStatus
// {1} = child.Name
// {2} = child notes
// {3} = type
static string RenderMemberStatus (ComparisonNode cn, string format)
{
	return String.Format (format, 
		MemberStatus (cn), 
		cn.Name,
		(cn.Missing > 0 ? ImageMissing : "") + (cn.Extra > 0 ? ImageExtra : ""),
		cn.Type.ToString ());
}

ComparisonNode ComparisonNodeFromTreeNode (TreeNode tn)
{
	if (tn.Parent == null){
		// This is needed because the tree loads chunks without calling Page_Load
		var cp = new global_asax.CompareParameters (Page.Request.QueryString);
		compare_context = cp.GetCompareContext ();

		return compare_context.Comparison;
	}
	
	var match = ComparisonNodeFromTreeNode (tn.Parent);
	if (match == null)
		return null;
	foreach (var n in match.Children){
		if (n.Name == tn.Value)
			return n;
	}
	return null;
}

// uses for class, struct, enum, interface
static string GetFQN (ComparisonNode node)
{
	if (node.Parent == null)
		return "";

	string n = GetFQN (node.Parent);
	int p = node.Name.IndexOf (' ');
	string name = p == -1 ? node.Name : node.Name.Substring (p+1);

	return n == "" ? name : n + "." + name;
}

// used for methods
static string GetMethodFQN (ComparisonNode node)
{
	if (node.Parent == null)
		return "";

	int p = node.Name.IndexOf ('(');
	int q = node.Name.IndexOf (' ');
	
	string name = p == -1 || q == -1 ? node.Name : node.Name.Substring (q+1, p-q-1);
	
	if (name == ".ctor")
		name = "";

	string n = GetFQN (node.Parent);
	return n == "" ? name : n + (name == "" ? "" : "." + name);
}

static string MakeURL (string type)
{
	return "http://msdn.microsoft.com/en-us/library/" + type.ToLower () + ".aspx";
}

static TreeNode MakeContainer (string kind, ComparisonNode node)
{
	TreeNode tn = new TreeNode (String.Format ("{0} {1} {2}", MemberStatus (node), kind, GetStatus (node)), node.Name);
	
	tn.SelectAction = TreeNodeSelectAction.None;
	return tn;
}

static void AttachComments (TreeNode tn, ComparisonNode node)
{
	if (node.Messages.Count != 0){
		TreeNode m = new TreeNode (GetMessages (node));
		m.SelectAction = TreeNodeSelectAction.None;
		tn.ChildNodes.Add (m);
	}
	if (node.Todos.Count != 0){
		TreeNode m = new TreeNode (GetTodo (node));
		tn.ChildNodes.Add (m);
	}
}

void TreeNodePopulate (object sender, TreeNodeEventArgs e)
{
	ComparisonNode cn = ComparisonNodeFromTreeNode (e.Node);
	if (cn == null){
		Console.WriteLine ("ERROR: Did not find the node");
		return;
	}

	foreach (var child in cn.Children){
		TreeNode tn;

		switch (child.Type){
		case CompType.Namespace:
			tn = new TreeNode (GetStatus (child), child.Name);
			tn.SelectAction = TreeNodeSelectAction.None;
			break;

		case CompType.Class:
		        tn = MakeContainer ("class", child);
			break;

		case CompType.Struct:
		        tn = MakeContainer ("struct", child);
			break;
			
		case CompType.Interface:
		        tn = MakeContainer ("interface", child);
			break;
			
		case CompType.Enum:
		        tn = MakeContainer ("enum", child);
			break;

		case CompType.Method:
			tn = new TreeNode (RenderMemberStatus (child, "{0}{1}{2}"), child.Name);
			AttachComments (tn, child);
			switch (cn.Type){
			case CompType.Property:
			        tn.NavigateUrl = MakeURL (GetFQN (cn));
				break;

			default:
				tn.NavigateUrl = MakeURL (GetMethodFQN (child));
				break;
			}
			tn.Target = "_blank";
			break;
			
		case CompType.Property:
		case CompType.Field:
		case CompType.Delegate:
		case CompType.Event:
			tn = new TreeNode (RenderMemberStatus (child, "{0} {3} {1}{2}"), child.Name);
			AttachComments (tn, child);

			// Fields whose parents are an enum are enum definitions, make the link useful
			if (child.Type == CompType.Field && cn.Type == CompType.Enum){
			   	tn.NavigateUrl = MakeURL (GetFQN (cn));
			} else 
				tn.NavigateUrl = MakeURL (GetFQN (child));
			tn.Target = "_blank";
			break;

		case CompType.Assembly:
		case CompType.Attribute:
			tn = new TreeNode (RenderMemberStatus (child, "{0} {3} {1}{2}"), child.Name);
			break;

		default:
			tn = new TreeNode ("Unknown type: " + child.Type.ToString());
			break;
		}

		if (child.Children.Count != 0)
			tn.PopulateOnDemand = true;
		
		e.Node.ChildNodes.Add (tn);
	}
}
</script>
<head>
  <title>Mono <%=compare_param.Assembly%>.dll API Compare against <%=compare_param.InfoDir%>
<style type="text/css">
.icons {
  width: 12px;
  height: 1em;
  display: inline-block;
  background: no-repeat left bottom;
}

.creport {
	display: inline-block;
	cursor: pointer;
}

.report {
	display: inline-block;
}

.suffix {
	margin-left: 0.5em;
}
	  
.missing {
	background-image: url(sm.gif);
}

.extra {
	background-image: url(sx.gif);
}

.ok {
	background-image: url(sc.gif);
}

.warning {
	background-image: url(mn.png);
}

.niex {
	background-image: url(se.gif);
}

.todo {
	background-image: url(st.gif);
}

  </style>
</head>

<body>
    Page generated at: <%=DateTime.Now %>
    <p>
    <form id="form" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Always">
            <ContentTemplate>
	        <div runat="server" id="waitdiv">
                  <img src="wait.gif" runat="server" enableviewstate="false" /> Loading and Comparing...
		</div>
		<div runat="server" id="activediv" >
		  <asp:Label id="time_label" runat="server"/>
	          <asp:TreeView ID="tree" Runat="server" OnTreeNodePopulate="TreeNodePopulate"
	          EnableClientScript="true"
	          PopulateNodesFromClient="true"
	          ExpandDepth="1">
	          </asp:TreeView>
		</div>
            </ContentTemplate>   
        </asp:UpdatePanel>
    </div>
    </form>
</body>
</html>