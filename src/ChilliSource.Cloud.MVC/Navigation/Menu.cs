using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    //If root is an area it's area value MUST be set. Roots must be defined before any children
    //First node is RootNode
    /// <summary>
    /// Represents the base functionality for all menus.
    /// </summary>
    public abstract class MenuBase
    {
        /// <summary>
        /// Root node.
        /// </summary>
        public static MenuNode RootNode = new MenuNode();
        /// <summary>
        /// Menu type.
        /// </summary>
        public static Type MenuType;

        #region Build
        //Call from Application_Start()
        /// <summary>
        /// Construct menus by menu type.
        /// </summary>
        /// <param name="menuType">The type.</param>
        public static void Build(Type menuType)
        {
            MenuType = menuType;

            var fields = menuType.GetFields();
            if (fields.Count() > 0) MenuBase.RootNode = fields[0].GetValue(null) as MenuNode;
            foreach (var field in fields)
            {
                BuildChildren(field, fields);
            }
        }

        /// <summary>
        /// Construct Relationship between parent and children nodes.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="children">The list of children nodes.</param>
        public static void SetCustomRelationship(MenuNode parent, List<MenuNode> children)
        {
            parent.Children = children;
            children.ForEach(c => c.Parent = parent);
        }

        /// <summary>
        /// Constructs children menu nodes.
        /// </summary>
        /// <param name="parentField">The parent node.</param>
        /// <param name="fields">The fields attributes.</param>
        private static void BuildChildren(FieldInfo parentField, FieldInfo[] fields)
        {
            var parentValue = parentField.GetValue(null) as MenuNode;
            parentValue.Id = parentValue.Id.DefaultTo(parentField.Name);

            //Set root value default. If root is an area it's value MUST be set. Roots must be defined before any children
            if (String.IsNullOrEmpty(parentValue.Controller))
            {
                parentValue.Controller = GetControllerFromName(parentField.Name, parentValue.Area);
            }

            foreach (var field in fields)
            {
                if (field.Name.StartsWith(parentField.Name) && field.Name != parentField.Name)
                {
                    if (field.Name.LastIndexOf('_') == parentField.Name.Length)
                    {
                        var fieldValue = field.GetValue(null) as MenuNode;

                        fieldValue.Id = fieldValue.Id.DefaultTo(field.Name);
                        fieldValue.Area = fieldValue.Area.DefaultTo(parentValue.Area);
                        fieldValue.Controller = fieldValue.Controller.DefaultTo(parentValue.Controller, GetControllerFromName(field.Name, parentValue.Area));
                        if (String.IsNullOrEmpty(parentValue.Controller) || parentValue.Controller == GetControllerFromName(field.Name, parentValue.Area))
                        {
                            fieldValue.Action = fieldValue.Action.DefaultTo(GetActionFromName(field.Name, parentValue.Area));
                        }
                        else
                        {   //Controller has been manually changed - reset depth of action name
                            //Find parent that was changed
                            var changedParent = parentValue;
                            while (changedParent.Parent != null && changedParent.Parent.Controller == fieldValue.Controller) changedParent = changedParent.Parent;
                            fieldValue.Action = GetActionFromName(field.Name.Substring(changedParent.Id.Length), "");
                        }
                        fieldValue.Parent = parentValue;

                        parentValue.Children.Add(fieldValue);
                    }
                }
            }
        }

        private static string GetControllerFromName(string menuName, string parentArea)
        {
            var nameParts = menuName.Split('_');
            if (String.IsNullOrEmpty(parentArea))
            {
                return nameParts[0];
            }
            else
            {
                return (nameParts.Length > 1) ? nameParts[1] : "";
            }

        }

        private static string GetActionFromName(string menuName, string parentArea)
        {
            var nameParts = menuName.Split('_');
            if (String.IsNullOrEmpty(parentArea))
            {
                if (nameParts.Length > 2) return String.Join("", nameParts, 1, nameParts.Length - 1);
                return (nameParts.Length > 1) ? nameParts[1] : "";
            }
            else
            {
                if (nameParts.Length > 3) return String.Join("", nameParts, 2, nameParts.Length - 2);
                return (nameParts.Length > 2) ? nameParts[2] : "";
            }

        }
        #endregion
        #region Get Menu
        /// <summary>
        /// Creates an instance of MenuNode by area, controller and action.
        /// </summary>
        /// <param name="area">The area name.</param>
        /// <param name="controller">The Controller Name.</param>
        /// <param name="action">The action Name.</param>
        /// <returns>An object of MenuNode.</returns>
        public static MenuNode GetMenu(string area = "", string controller = "", string action = "")
        {
            var fields = MenuType.GetFields();
            var controllers = new List<MenuNode>();
            foreach (var field in fields)
            {
                var menuNode = field.GetValue(null) as MenuNode;
                if (menuNode.Action.Equals(action, StringComparison.OrdinalIgnoreCase) &&
                    menuNode.Controller.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                    menuNode.Area.Equals(area, StringComparison.OrdinalIgnoreCase))
                {
                    return menuNode.ReturnMe();
                }
                if (menuNode.Controller.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                    menuNode.Area.Equals(area, StringComparison.OrdinalIgnoreCase))
                {
                    controllers.Add(menuNode);
                }

            }
            if (controllers.Count > 0) return controllers[0];
            return new MenuNode();
        }

        /// <summary>
        /// Creates an instance of MenuNode of current menu.
        /// </summary>
        /// <returns>An object of MenuNode.</returns>
        public static MenuNode GetCurrentMenu()
        {
            var area = RouteHelper.CurrentArea() ?? "";
            var controller = RouteHelper.CurrentController() ?? "";
            var action = RouteHelper.CurrentAction() ?? "";

            return GetMenu(area, controller, action);
        }

        /// <summary>
        /// Creates an instance of MenuNode of referred menu.
        /// </summary>
        /// <returns>An object of MenuNode.</returns>
        public static MenuNode GetReferredMenu()
        {
            var routeData = RouteHelper.GetRouteDataByUrl(HttpContext.Current.Request.UrlReferrer).Values;
            return GetMenuFromRouteDate(routeData);
        }

        /// <summary>
        /// Creates an instance of MenuNode of return url menu.
        /// </summary>
        /// <returns>An object of MenuNode.</returns>
        public static MenuNode GetReturnUrlMenu()
        {
            var returnUrl = Authentication.GetReturnUrl();
            if (Authentication.IsValidReturnUrl(returnUrl))
            {
                var routeData = RouteHelper.GetRouteDataByUrl(returnUrl).Values;
                return GetMenuFromRouteDate(routeData);
            }
            return null;
        }

        /// <summary>
        /// Creates an instance of MenuNode from route data.
        /// </summary>
        /// <param name="routeData">A System.Web.Routing.RouteValueDictionary.</param>
        /// <returns>An object of MenuNode.</returns>
        public static MenuNode GetMenuFromRouteDate(RouteValueDictionary routeData)
        {
            var area = RouteHelper.CurrentArea(routeData) ?? "";
            var controller = RouteHelper.CurrentController(routeData) ?? "";
            var action = RouteHelper.CurrentAction(routeData) ?? "";
            return GetMenu(area, controller, action);
        }
        /// <summary>
        /// Creates the sibling MenuNode from action name.
        /// </summary>
        /// <param name="action">The action name.</param>
        /// <returns>An object of MenuNode.</returns>
        public static MenuNode GetSibling(string action)
        {
            var startNode = GetCurrentMenu();
            while (startNode.Parent != null) startNode = startNode.Parent;
            return GetSibling(startNode.Children, action);
        }

        /// <summary>
        /// Creates the sibling MenuNode from the nodes list and action name.
        /// </summary>
        /// <param name="action">The action name.</param>
        /// <param name="menuList">The list of menu nodes.</param>
        /// <returns>An object of MenuNode.</returns>
        public static MenuNode GetSibling(List<MenuNode> menuList, string action)
        {
            foreach (var menuNode in menuList)
            {
                if (menuNode.Action.Equals(action, StringComparison.OrdinalIgnoreCase))
                    return menuNode.ReturnMe();
                if (menuNode.Children != null)
                {
                    var result = GetSibling(menuNode.Children, action);
                    if (result != null) return result.ReturnMe();
                }
            }
            return null;
        }

        public static List<MenuNode> GetRootNodes(Type menuType)
        {
            var result = new List<MenuNode>();
            var fields = menuType.GetFields();
            foreach (var field in fields)
            {
                var menu = field.GetValue(null) as MenuNode;
                if (menu != null && menu.Parent == null)
                {
                    result.Add(menu);
                }
            }
            return result;
        }
        #endregion

        /// <summary>
        /// Creates an instance of RouteValueDictionary of current route from route values.
        /// </summary>
        /// <param name="routeValues">The route value parameters.</param>
        /// <returns>An object of RouteValueDictionary.</returns>
        public static RouteValueDictionary GetCurrentRouteParameters(object routeValues = null)
        {
            var parameters = HttpContext.Current.Request.RequestContext.RouteData.Values.Parameters(HttpContext.Current.Request.Url.Query);
            if (routeValues != null) parameters = parameters.Parameters(routeValues);
            return parameters;
        }

        //?active=Action|{id}|param1={x};active1= .....
        //returns { action = Action, id = {id}, param1 = {x}, active = .... }
        /// <summary>
        /// Creates an object of RouteValueDictionary of active command parameters.
        /// </summary>
        /// <param name="active">The active parameters.</param>
        /// <returns>An object of RouteValueDictionary.</returns>
        public static RouteValueDictionary GetActiveCommand(string active = "")
        {
            active = active.DefaultTo(HttpContext.Current.Request.QueryString["active"]);
            if (String.IsNullOrEmpty(active)) return null;

            var activeCommands = active.Split(';');

            var activeCommand = activeCommands[0].Split('|');

            var routeValues = new RouteValueDictionary();
            for (var i = 0; i < activeCommand.Length; i++)
            {
                if (i == 0) { routeValues.Add("action", activeCommand[i]); continue; }

                var param = activeCommand[i].Split('=');
                if (param.Length == 1) param = new string[] { "id", param[0] };
                routeValues.Add(param[0], param[1]);
            }

            if (activeCommands.Length > 1)
            {
                routeValues.Add("active", String.Join(";", activeCommands, 1, activeCommands.Length - 1));
            }

            return routeValues;
        }

    }

    /// <summary>
    /// Represents the base functionality for menu item.
    /// </summary>
    public class MenuNode
    {
        /// <summary>
        /// Initializes a new instance of the MenuNode class.
        /// </summary>
        public MenuNode()
        {
            Id = "";
            Area = "";
            Controller = "";
            Action = "";
            Title = "";
            Tag = "";
            Target = "";
            ReturnUrl = "";
            CssClass = "";

            Children = new List<MenuNode>();
        }

        /// <summary>
        /// The menu id.
        /// </summary>
        public string Id { get; set; }  // Can be used to set an id on a html tag etc

        /// <summary>
        /// The formatted menu id by type.
        /// </summary>
        /// <param name="type">The menu id type.</param>
        /// <returns>The formatted menu id.</returns>
        public string GetIdAs(MenuIdType type)
        {
            return String.Format("{0}_{1}", type, this.Id);
        }

        //Route
        /// <summary>
        /// The area name.
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// The controller name.
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// The action name.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The route name.
        /// </summary>
        public string RouteName { get; set; }
        private string _View;
        /// <summary>
        /// The View name.
        /// </summary>
        public string View  //return View(MyMenu.Project_Index.View)
        {
            get
            {
                return this.ReturnMe()._View.DefaultTo(Action);
            }
            set { _View = value; }
        }

        private string _Title;

        /// <summary>
        /// The menu title.
        /// </summary>
        public string Title
        {
            get
            {
                var action = (Parent == null || Parent.Action == Action) ? Action : Action.TrimStart(Parent.Action);
                return _Title.DefaultTo(action.ToSentenceCase(splitByUppercase: true), Controller.ToSentenceCase(splitByUppercase: true));
            }
            set { _Title = value; }
        }

        /// <summary>
        /// Used to populate title in action items like buttons. Defaults to this.Action[space]this.controller all in sentence case eg "Create project".
        /// </summary>
        private string _ActionTitle;
        /// <summary>
        /// The action title.
        /// </summary>
        public string ActionTitle
        {
            get { return _ActionTitle.DefaultTo(Title); }
            set { _ActionTitle = value; }
        }

        /// <summary>
        /// Parent menu node.
        /// </summary>
        public MenuNode Parent { get; set; }

        /// <summary>
        /// Children menu nodes.
        /// </summary>
        public List<MenuNode> Children { get; set; }

        /// <summary>
        /// The alias for menu node.
        /// </summary>
        public MenuNode Alias { get; set; }

        private string _Target;
        /// <summary>
        /// The menu target.
        /// </summary>
        public string Target { get { return _Target.DefaultTo("menu-target"); } set { _Target = value; } }

        /// <summary>
        /// The role name.
        /// </summary>
        public string Role { set { Roles = new List<string> { value }; } }

        /// <summary>
        /// The list of roles.
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// The tag name.
        /// </summary>
        public string Tag { set { Tags = new List<string> { value }; } }
        /// <summary>
        /// The list of tags.
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// The not tag name.
        /// </summary>
        public string NotTag { set { NotTags = new List<string> { value }; } }
        /// <summary>
        /// The list of not tags.
        /// </summary>
        public List<string> NotTags { get; set; }

        /// <summary>
        /// Resolve tags based on the list of tags and notTags.
        /// </summary>
        /// <param name="tags">A list of tags.</param>
        /// <param name="notTags">A list of not tags.</param>
        /// <returns>True if any item in tags exists and any item in not tags exists, otherwise False.</returns>
        public bool ResolveTags(List<string> tags = null, List<string> notTags = null)
        {
            return (tags == null || Tags == null || Tags.Intersect(tags).Count() > 0) && (notTags == null || NotTags == null || NotTags.Intersect(notTags).Count() == 0);
        }

        private string _ReturnUrl;
        /// <summary>
        /// The redirect url.
        /// </summary>
        public string ReturnUrl
        {
            get { return HttpContext.Current.Request.RequestContext.TransformUrl(_ReturnUrl); }
            set { _ReturnUrl = value; }
        }
        /// <summary>
        /// Get the redirect url.
        /// </summary>
        /// <param name="transformWith">Properties in this object will be used to replace placeholders.</param>
        /// <returns>Redirect url.</returns>
        public string GetReturnUrl(object transformWith)
        {
            return _ReturnUrl.TransformWith(transformWith);
        }
        private string _CssClass;
        /// <summary>
        /// Get the CssClass for this menu, generally this is used in the layout container so that targeting particular pages in css is made easy.
        /// </summary>
        public string CssClass
        {
            get { return _CssClass.DefaultTo(Id.ToCssClass()); }
            set { _CssClass = value; }
        }
        /// <summary>
        /// Get the icon name.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Get the icon name formatted with the icon class.
        /// </summary>
        /// <param name="iconClasses">The icon class name.</param>
        /// <returns>Returns formatted icon name.</returns>
        private string ResolveIcon(string iconClasses)
        {
            if (iconClasses.Equals("icon-white") && !String.IsNullOrEmpty(Icon)) return "{0} {1}".FormatWith(Icon, iconClasses);
            return iconClasses.DefaultTo(Icon);
        }

        /// <summary>
        /// Get menu title.
        /// </summary>
        /// <returns>Menu title.</returns>
        public override string ToString()
        {
            return Title;
        }

        #region Url / Uri / RouteValue / Is Current
        /// <summary>
        /// Get menu link by id, route values, protocol and host name.
        /// </summary>
        /// <param name="id">The menu id.</param>
        /// <param name="routeValues">The route values.</param>
        /// <param name="protocol">The protocol name.</param>
        /// <param name="hostName">The host name.</param>
        /// <returns>Menu link.</returns>
        public string Url(int id, object routeValues = null, string protocol = "", string hostName = "")
        {
            return Url(id.ToString(), routeValues, protocol, hostName);
        }

        /// <summary>
        /// Get menu link by id, route values, protocol and host name.
        /// </summary>
        /// <param name="id">The menu id.</param>
        /// <param name="routeValues">The route values.</param>
        /// <param name="protocol">The protocol name.</param>
        /// <param name="hostName">The host name.</param>
        /// <returns>Menu link.</returns>
        public string Url(string id = "", object routeValues = null, string protocol = "", string hostName = "")
        {
            MenuNode menuNode = this.ReturnMe();

            var url = UrlHelperExtensions.Create();
            return url.DefaultAction(menuNode.Action, menuNode.Controller, menuNode.Area, menuNode.RouteName, id, routeValues, protocol, hostName);
        }

        /// <summary>
        /// Get menu URI by id and route values.
        /// </summary>
        /// <param name="id">The menu id.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>Menu link.</returns>
        public Uri Uri(string id = "", object routeValues = null)
        {
            return UriExtensions.Parse(Url(id, routeValues));
        }

        /// <summary>
        /// Get menu URI by id and route values.
        /// </summary>
        /// <param name="id">The menu id.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>Menu URI.</returns>
        public Uri Uri(int id, object routeValues = null)
        {
            return UriExtensions.Parse(Url(id, routeValues));
        }

        /// <summary>
        /// Creates an instance of RouteValueDictionary from id and route value parameters.
        /// </summary>
        /// <param name="id">The value of id in the route values.</param>
        /// <param name="routeValues">The route value parameters.</param>
        /// <returns>An object of RouteValueDictionary.</returns>
        public RouteValueDictionary RouteValues(int id, object routeValues = null)
        {
            return RouteValues(id.ToString(), routeValues);
        }

        /// <summary>
        /// Creates an instance of RouteValueDictionary from id and route value parameters.
        /// </summary>
        /// <param name="id">The value of id in the route values.</param>
        /// <param name="routeValues">The route value parameters.</param>
        /// <returns>An object of RouteValueDictionary.</returns>
        public RouteValueDictionary RouteValues(string id = "", object routeValues = null)
        {
            MenuNode menuNode = this.ReturnMe();
            var routeValuesDictionary = routeValues as RouteValueDictionary;
            if (routeValuesDictionary == null) routeValuesDictionary = new RouteValueDictionary(routeValues);
            routeValuesDictionary["area"] = menuNode.Area;
            routeValuesDictionary["controller"] = menuNode.Controller;
            routeValuesDictionary["action"] = menuNode.Action;
            if (!String.IsNullOrEmpty(id)) routeValuesDictionary["id"] = id;
            return routeValuesDictionary;
        }

        /// <summary>
        /// Returns true for current menu.
        /// </summary>
        /// <returns>True if it's current menu; otherwise, False.</returns>
        public bool IsCurrent()
        {
            var current = MenuBase.GetCurrentMenu();
            return (current.Id == this.Id);
        }

        /// <summary>
        /// Creates an HTML-encoded string using the specified result for current node.
        /// </summary>
        /// <param name="resultWhenTrue">The specified result for current node.</param>
        /// <returns>An object of MvcHtmlString.</returns>
        public MvcHtmlString WhenCurrent(string resultWhenTrue)
        {
            return IsCurrent() ? MvcHtmlString.Create(resultWhenTrue) : MvcHtmlString.Empty;
        }

        /// <summary>
        /// Checks if the specified URL match the route data in the System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="routeValues">Route values to be checked.</param>
        /// <returns>True if the specified URL match the route data in the System.Web.Mvc.UrlHelper; otherwise, False.</returns>
        public bool IsActive(object routeValues = null)
        {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            if (urlHelper.IsCurrent(Url(routeValues: routeValues)))
            {
                return true;
            }
            if (Children.Count == 0)
            {
                return false;
            }
            return Children.Any(c => c.IsActive(routeValues));
        }

        #endregion

        #region Link
        /// <summary>
        /// Returns HTML string for the link element from title, id, routeValues, linkClasses, iconClasses and linkAttributes.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString Link(int id, string title = "", object routeValues = null, string linkClasses = "", string iconClasses = "", object linkAttributes = null)
        {
            return Link(title, id.ToString(), routeValues, linkClasses, iconClasses, linkAttributes);
        }

        /// <summary>
        /// Returns HTML string for the link element from title, id, routeValues, linkClasses, iconClasses and linkAttributes.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString Link(string title = "", string id = "", object routeValues = null, string linkClasses = "", string iconClasses = "", object linkAttributes = null)
        {
            return Link(new HtmlLinkFieldOptions { Title = title, Id = id, RouteValues = routeValues, LinkClasses = linkClasses, IconClasses = iconClasses, HtmlAttributes = linkAttributes });
        }

        /// <summary>
        /// Returns HTML string for the link element from link html attributes.
        /// </summary>
        /// <param name="options">The link html attributes.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString Link(HtmlLinkFieldOptions options)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var menu = ReturnMe();
            return HtmlHelperExtensions.Link(url, menu.Action, menu.Controller, menu.Area, menu.RouteName, options.Id, options.RouteValues, options.Title == String.Empty ? menu.Title : options.Title, options.LinkClasses, this.ResolveIcon(options.IconClasses), options.HtmlAttributes, options.HostName);
        }

        /// <summary>
        /// Returns HTML string for the link element to perform post action.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="confirmFunction">JavaScript function to confirm before from post.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString LinkPost(int id, string title = "", object routeValues = null, string linkClasses = "", string iconClasses = "", object linkAttributes = null, string confirmFunction = "")
        {
            return LinkPost(title, id.ToString(), routeValues, linkClasses, iconClasses, linkAttributes, confirmFunction);
        }

        /// <summary>
        /// Returns HTML string for the link element to perform post action.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="confirmFunction">JavaScript function to confirm before from post.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString LinkPost(string title = "", string id = "", object routeValues = null, string linkClasses = "", string iconClasses = "", object linkAttributes = null, string confirmFunction = "")
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var menu = ReturnMe();
            return HtmlHelperExtensions.LinkPost(url, menu.Action, menu.Controller, menu.Area, menu.RouteName, id, routeValues, title == String.Empty ? menu.Title : title, linkClasses, this.ResolveIcon(iconClasses), linkAttributes, confirmFunction);
        }

        /// <summary>
        /// Returns HTML string for the link element to perform Ajax asynchronous request.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="displayText">The text to display for the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="target">The ID of the HTML element to be rendered after Ajax asynchronous request succeeded.</param>
        /// <param name="callbackJs">JavaScript function to execute after Ajax asynchronous request succeeded.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString LinkAjax(int id, object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string dynamicData = "", string target = null, string callbackJs = "")
        {
            return LinkAjax(id.ToString(), routeValues, displayText, linkClasses, iconClasses, linkAttributes, dynamicData, target: target, callbackJs: callbackJs);
        }

        /// <summary>
        /// Returns HTML string for the link element to perform Ajax asynchronous request.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="displayText">The text to display for the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="target">The ID of the HTML element to be rendered after Ajax asynchronous request succeeded.</param>
        /// <param name="callbackJs">JavaScript function to execute after Ajax asynchronous request succeeded.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString LinkAjax(string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string dynamicData = "", string target = null, string callbackJs = "")
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return HtmlHelperExtensions.LinkAjax(url, target.DefaultTo(this.Target), this.Action, this.Controller, this.Area, this.RouteName, id, routeValues, displayText == String.Empty ? this.Title : displayText, linkClasses, this.ResolveIcon(iconClasses), linkAttributes, dynamicData, callbackJs: callbackJs);
        }
        #endregion

        #region Paging
        /// <summary>
        /// Wire up the pager control emitted from Html.PagerFor or Html.PagerLoopPrevious / Next.
        /// </summary>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="pagerId">Use if more than 1 pager control.</param>
        /// <param name="isAjax">True to load page via Ajax, otherwise not.</param>
        /// <param name="pageDataFunction">(Unfortunately named) OBJECT that will add / override query string parameters for the request. Is evaluated when the user clicks.</param>
        /// <param name="successFunction">Call back function on successful load (only if ajax).</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString PagerWireUp(object routeValues = null, int? pagerId = null, bool isAjax = false, string pageDataFunction = null, string successFunction = null)
        {
            var rvd = RouteValueDictionaryExtensions.Create(routeValues);
            rvd.Remove("page");
            var uri = this.Uri(routeValues: rvd);
            var url = uri.PathAndQuery;
            var selector = pagerId.HasValue ? "pager" + pagerId.Value.ToString() : "pagination a";
            if (isAjax)
            {
                var format = "$('.{0}').unbind('click').bind('click', function () {{ $.ajaxLoad('{1}', '{2}', {3}{4}); $.onAjaxStart('{1}');}});";
                return MvcHtmlString.Create(String.Format(format, selector, this.Target, url,
                    String.IsNullOrEmpty(pageDataFunction) ? "$(this).data()" : "$.extend($(this).data(), " + pageDataFunction + ")",
                    String.IsNullOrEmpty(successFunction) ? "" : ", " + successFunction));
            }
            else if (!string.IsNullOrWhiteSpace(pageDataFunction))
            {
                var format = "$('.{0}').unbind('click').bind('click', function () {{ window.location.href = '{1}?' + {2}; }});";
                // http://stackoverflow.com/questions/8648892/convert-url-parameters-to-a-javascript-object/13366851#13366851
                var currentParams = string.IsNullOrWhiteSpace(uri.Query) ? "{}" :
                        @"JSON.parse('{{""' + decodeURI({0}).replace(/""/g, '\\""').replace(/&/g, '"",""').replace(/=/g,'"":""') + '""}}')".FormatWith(uri.Query);
                var query = "$.param($.extend($(this).data(), {0}, {1}))".FormatWith(currentParams, pageDataFunction);

                return MvcHtmlString.Create(String.Format(format, selector, uri.AbsolutePath, query));
            }
            else
            {
                var format = "$('.{0}').unbind('click').bind('click', function () {{ window.location.href = '{1}' + $.param($(this).data()); }});";
                return MvcHtmlString.Create(String.Format(format, selector, url + (String.IsNullOrEmpty(uri.Query) ? "?" : "&")));
            }
        }

        /// <summary>
        /// Returns HTML string for the pager active action.
        /// </summary>
        /// <param name="pagerId">Use if more than 1 pager control.</param>
        /// <param name="isAjax">True to load page via Ajax, otherwise not.</param>
        /// <param name="active">The active command.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString PagerActiveAction(int? pagerId = null, bool isAjax = false, string active = "")
        {
            var result = MvcHtmlString.Empty;
            var routeValues = MenuBase.GetActiveCommand(active);
            if (routeValues == null) return result;
            var selector = pagerId.HasValue ? "pager" + pagerId.Value.ToString() : "pagination a";
            if (isAjax)
            {
                var setData = new StringBuilder();
                foreach (var key in routeValues.Keys) if (key != "action" && key != "page") setData.AppendFormat(".data('{0}', '{1}')", key, routeValues[key]);
                if (routeValues["page"] == null) routeValues["page"] = "1";
                return result.Format("$('.{0}:[data-page={1}]'){2}.click();", selector, routeValues["page"], setData.ToString());
            }
            return result;
        }
        #endregion

        #region Menu
        /// <summary>
        /// Returns HTML string for the menu created from tag, notTag, visibility and routeValues.
        /// </summary>
        /// <param name="tag">The menu tag.</param>
        /// <param name="notTag">The menu not tag.</param>
        /// <param name="visibility">The menu node visibility.</param>
        /// <param name="routeValues">The route value parameters.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString Menu(string tag = "", string notTag = "", IMenuNodeVisibility visibility = null, object routeValues = null)
        {
            List<string> tags = String.IsNullOrEmpty(tag) ? null : new List<string> { tag };
            List<string> notTags = String.IsNullOrEmpty(notTag) ? null : new List<string> { notTag };
            return Menu(tags, notTags, visibility, routeValues);
        }

        /// <summary>
        /// Returns HTML string for the menu created from tag, notTag, visibility and routeValues.
        /// </summary>
        /// <param name="tags">The list of menu tags</param>
        /// <param name="notTags">The list of menu not tags</param>
        /// <param name="visibility">The menu node visibility.</param>
        /// <param name="routeValues">The route value parameters.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString Menu(List<string> tags, List<string> notTags = null, IMenuNodeVisibility visibility = null, object routeValues = null)
        {
            var sb = new StringBuilder();
            foreach (var child in this.Children)
            {
                if (!child.ResolveTags(tags, notTags)) continue;
                if (!Authentication.IsInAnyRole(child.Roles)) continue;
                if (!child.IsVisible(visibility)) continue;

                sb.AppendLine(child.MenuItem(routeValues: routeValues).ToHtmlString());
            }
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// Returns HTML string for the menu item created from link html attributes and list item attributes.
        /// </summary>
        /// <param name="linkOptions">The link html attributes.</param>
        /// <param name="listItemOptions">The list item attributes.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString MenuItem(HtmlLinkFieldOptions linkOptions, HtmlListItemFieldOptions listItemOptions = null)
        {
            if (!Authentication.IsInAnyRole(this.Roles)) return MvcHtmlString.Empty;

            if (linkOptions == null) linkOptions = new HtmlLinkFieldOptions();
            if (listItemOptions == null) listItemOptions = new HtmlListItemFieldOptions();

            TagBuilder liTag = new TagBuilder("li");
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(listItemOptions.HtmlAttributes);
            liTag.MergeAttributes(attributes);

            liTag.InnerHtml = Link(linkOptions).ToHtmlString();

            if (IsActive(linkOptions.RouteValues))
            {
                liTag.AddCssClass("active");
            }

            return MvcHtmlString.Create(liTag.ToString());
        }

        /// <summary>
        /// Returns HTML string for the menu item created from title, route values, link css classes and visibility attribute.
        /// </summary>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">The route value parameters.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="visibility">The menu node visibility provider.</param>
        /// <param name="statusProvider">The menu node status provider.</param>
        /// <returns>HTML string for the link.</returns>
        public MvcHtmlString MenuItem(string title = "", object routeValues = null, string linkClasses = null, IMenuNodeVisibility visibility = null, IMenuNodeStatus statusProvider = null)
        {
            if (!Authentication.IsInAnyRole(this.Roles) || !this.IsVisible(visibility))
                return MvcHtmlString.Empty;

            TagBuilder liTag = new TagBuilder("li");

            liTag.InnerHtml = Link(title: title, routeValues: routeValues, linkClasses: linkClasses).ToHtmlString();

            if ((statusProvider == null && IsActive(routeValues))
              || (statusProvider != null && statusProvider.IsActive(this, routeValues)))
            {
                liTag.AddCssClass("active");
            }
            return MvcHtmlString.Create(liTag.ToString());
        }

        /// <summary>
        /// Returns true if menu node is visible.
        /// </summary>
        /// <param name="visibility">The menu node visibility.</param>
        /// <returns>True if menu node is visible; otherwise, False.</returns>
        private bool IsVisible(IMenuNodeVisibility visibility)
        {
            if (visibility == null)
                return true;

            if (this.Alias != null)
                return this.Alias.IsVisible(visibility);

            return visibility.IsVisible(this);
        }
        #endregion

        #region NavTab
        /// <summary>
        /// Gets list of Nav Tab Items.
        /// </summary>
        /// <param name="tag">The tag value.</param>
        /// <param name="notTag">The not tag value.</param>
        /// <param name="firstIsActive">True if first item is active.</param>
        /// <param name="visibility">The tab item visibility.</param>
        /// <returns>Returns list of NavTabItem.</returns>
        public List<NavTabItem> NavTabItems(string tag = "", string notTag = "", bool firstIsActive = false, IMenuNodeVisibility visibility = null)
        {
            List<string> tags = String.IsNullOrEmpty(tag) ? null : new List<string> { tag };
            List<string> notTags = String.IsNullOrEmpty(notTag) ? null : new List<string> { notTag };
            return NavTabItems(tags, notTags, firstIsActive, visibility);
        }

        /// <summary>
        /// Gets list of Nav Tab Items.
        /// </summary>
        /// <param name="tags">The list of tag values.</param>
        /// <param name="notTags">The list of not tag values.</param>
        /// <param name="firstIsActive">True if first item is active.</param>
        /// <param name="visibility">The tab item visibility.</param>
        /// <returns>Returns list of NavTabItem.</returns>
        public List<NavTabItem> NavTabItems(List<string> tags, List<string> notTags = null, bool firstIsActive = false, IMenuNodeVisibility visibility = null)
        {
            //if (tags == null) tags = new List<string>();
            var result = new List<NavTabItem>();
            foreach (var child in this.Children)
            {
                if (!child.ResolveTags(tags, notTags)) continue;
                if (!Authentication.IsInAnyRole(child.Roles)) continue;
                if (!child.IsVisible(visibility)) continue;

                result.Add(new NavTabItem(child));
            }
            if (firstIsActive) result[0].IsActive = true;
            return result;
        }
        #endregion

        /// <summary>
        /// Retrieve a list of parent menu nodes starting from the First.
        /// </summary>
        /// <param name="childMenuNode">The child menu item.</param>
        /// <returns>A list of parent menu nodes.</returns>
        public List<MenuNode> GetAncestry(MenuNode childMenuNode)
        {
            var result = new List<MenuNode>();

            while (childMenuNode.Parent != null)
            {
                result.Insert(0, childMenuNode.Parent);
                childMenuNode = childMenuNode.Parent;
            }
            return result;
        }

        #region Button
        /// <summary>
        /// Returns HTML string for the button element from title, id, routeValues, buttonClasses, iconClasses and buttonAttributes.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate id of button element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>HTML string for the button.</returns>

        public MvcHtmlString Button(string title = "", string id = "", object routeValues = null, string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return HtmlHelperExtensions.Button(url, this.Action, this.Controller, this.Area, this.RouteName, id, routeValues, title == String.Empty ? this.ActionTitle : title, buttonClasses, this.ResolveIcon(iconClasses), buttonAttributes);
        }

        /// <summary>
        /// Returns HTML string for the button element from title, id, routeValues, buttonClasses, iconClasses and buttonAttributes.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate id of button element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>HTML string for the button.</returns>
        public MvcHtmlString Button(int id, string title = "", object routeValues = null, string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            return Button(title, id.ToString(), routeValues, buttonClasses, iconClasses, buttonAttributes);
        }

        /// <summary>
        /// Returns HTML string for the button element to perform post action.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate id of button element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="confirmFunction">JavaScript function to confirm before from post.</param>
        /// <returns>HTML string for the button.</returns>
        public MvcHtmlString ButtonPost(string title = "", string id = "", object routeValues = null, string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, string confirmFunction = "")
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return HtmlHelperExtensions.ButtonPost(url, this.Action, this.Controller, this.Area, this.RouteName, id, routeValues, title == String.Empty ? this.ActionTitle : title, buttonClasses, this.ResolveIcon(iconClasses), buttonAttributes, confirmFunction);
        }

        /// <summary>
        /// Returns HTML string for the button element to perform post action.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate id of button element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="confirmFunction">JavaScript function to confirm before from post.</param>
        /// <returns>HTML string for the button.</returns>
        public MvcHtmlString ButtonPost(int id, string title = "", object routeValues = null, string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, string confirmFunction = "")
        {
            return ButtonPost(title, id.ToString(), routeValues, buttonClasses, iconClasses, buttonAttributes, confirmFunction);
        }

        /// <summary>
        /// Returns HTML string for the button element to perform submit action.
        /// </summary>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>HTML string for the button.</returns>
        public MvcHtmlString ButtonSubmit(string title = "", object routeValues = null, string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return HtmlHelperExtensions.ButtonSubmit(url, title == String.Empty ? this.ActionTitle : title, buttonClasses, this.ResolveIcon(iconClasses), buttonAttributes);
        }

        /// <summary>
        /// Returns HTML string for the disabled button element.
        /// </summary>
        /// <param name="helpText">The text to display in "alert" popup window when button click event triggered.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="buttonClasses">The CSS class for the disabled button element.</param>
        /// <param name="iconClasses">The CSS icon class for the disabled button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <returns>HTML string for the disabled button.</returns>
        public MvcHtmlString ButtonDisabled(string helpText = "", string title = "", string buttonClasses = "", string iconClasses = "", object buttonAttributes = null)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return HtmlHelperExtensions.ButtonDisabled(url, title == String.Empty ? this.ActionTitle : title, helpText, buttonClasses, iconClasses, buttonAttributes);
        }

        /// <summary>
        /// Returns HTML string for the button element to perform Ajax asynchronous request.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate id of the button element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="post">True if action is "POST".</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="target">The ID of the HTML element to be rendered after Ajax asynchronous request succeeded.</param>
        /// <param name="callbackJs">JavaScript function to execute after Ajax asynchronous request succeeded.</param>
        /// <returns>HTML string for the button.</returns>
        public MvcHtmlString ButtonAjax(string title = "", string id = "", object routeValues = null, string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, bool post = false, string dynamicData = "", string target = null, string callbackJs = "")
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return HtmlHelperExtensions.ButtonAjax(url, target.DefaultTo(this.Target), this.Action, this.Controller, this.Area, this.RouteName, id, routeValues, title == String.Empty ? this.ActionTitle : title, buttonClasses, this.ResolveIcon(iconClasses), buttonAttributes, post, dynamicData, callbackJs: callbackJs);
        }

        /// <summary>
        /// Returns HTML string for the button element to perform Ajax asynchronous request.
        /// </summary>
        /// <param name="id">The value of ID in the route values used to generate id of the button element.</param>
        /// <param name="title">The value of title in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the button element.</param>
        /// <param name="buttonClasses">The CSS class for the button element.</param>
        /// <param name="iconClasses">The CSS icon class for the button element.</param>
        /// <param name="buttonAttributes">An object that contains the HTML attributes to set for the button element.</param>
        /// <param name="post">True if action is "POST".</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="target">The ID of the HTML element to be rendered after Ajax asynchronous request succeeded.</param>
        /// <param name="callbackJs">JavaScript function to execute after Ajax asynchronous request succeeded.</param>
        /// <returns>HTML string for the button.</returns>
        public MvcHtmlString ButtonAjax(int id, string title = "", object routeValues = null, string buttonClasses = "", string iconClasses = "", object buttonAttributes = null, bool post = false, string dynamicData = "", string target = null, string callbackJs = "")
        {
            return ButtonAjax(title, id.ToString(), routeValues, buttonClasses, iconClasses, buttonAttributes, post, dynamicData, target: target, callbackJs: callbackJs);
        }

        #endregion

        #region Redirect
        /// <summary>
        /// Controls the processing of application actions by redirecting to a specified URI.
        /// </summary>
        /// <param name="id">The value of ID in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="protocol">The protocol name.</param>
        /// <returns>An instance of the RedirectResult class.</returns>
        public RedirectResult Redirect(string id = "", object routeValues = null, string protocol = "")
        {
            var httpContext = (HttpContext.Current == null) ? null : HttpContext.Current.Request.RequestContext.HttpContext;
            var redirectTo = this.Url(id, routeValues, protocol);

            if (httpContext != null && httpContext.Request.IsAjaxRequest())
            {
                httpContext.Response.Headers["X-Ajax-Redirect"] = redirectTo;
                return null;
            }
            return new RedirectResult(redirectTo);
        }

        /// <summary>
        /// Controls the processing of application actions by redirecting to a specified URI.
        /// </summary>
        /// <param name="id">The value of ID in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="protocol">The protocol name.</param>
        /// <returns>An instance of the RedirectResult class.</returns>
        public RedirectResult Redirect(int id, object routeValues = null, string protocol = "")
        {
            return Redirect(id.ToString(), routeValues, protocol);
        }
        #endregion

        #region Modal
        /// <summary>
        /// Returns HTML string for the modal element.
        /// </summary>
        /// <param name="actionTitle">The title of the link.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the modal.</param>
        /// <param name="width">The modal width.</param>
        /// <param name="height">The modal height.</param>
        /// <param name="iconClasses">The CSS icon class for the modal element.</param>
        /// <param name="htmlAttributes">The specified html attribute object.</param>
        /// <param name="commandOnly">If false, add "onclick" attribute to modal.</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="BackgroundDrop">True to add CSS "backdrop: 'static'", otherwise not.</param>
        /// <param name="EscapeKey">True to add CSS "keyboard: false".</param>
        /// <returns>HTML string for the modal.</returns>
        public MvcHtmlString ModalOpen(string actionTitle = "", object routeValues = null, int? width = null, int? height = null, string iconClasses = "", object htmlAttributes = null, bool commandOnly = false, string dynamicData = "", bool BackgroundDrop = true, bool EscapeKey = true)
        {
            return MvcHtmlString.Create(ModalHelper.ModalOpen(this.GetIdAs(MenuIdType.Modal), Url(routeValues: routeValues), actionTitle.DefaultTo(this.ActionTitle), width, height, this.ResolveIcon(iconClasses), htmlAttributes, commandOnly, dynamicData, BackgroundDrop, EscapeKey));
        }

        #endregion

        #region Misc Helpers
        /// <summary>
        /// Get menu node or its Alias.
        /// </summary>
        /// <returns>An instance of MenuNode class.</returns>
        public MenuNode ReturnMe()
        {
            return this.Alias == null ? this : this.Alias;
        }
        #endregion
    }

    /// <summary>
    /// Represents the base functionality for menu html helper.
    /// </summary>
    public static class MenuHtmlHelper
    {
        /// <summary>
        /// Writes an opening <form> tag to the response and includes the route values in the action attribute. 
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="menuNode">The menu node to get the form Id, action and controller.</param>
        /// <param name="htmlAttributes">The specified html attribute object.</param>
        /// <returns>An instance of MvcForm object.</returns>
        public static MvcForm BeginForm<TModel>(this HtmlHelper<TModel> htmlHelper, MenuNode menuNode, object htmlAttributes = null)
        {
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);
            if (!attributes.ContainsKey("id")) attributes["id"] = menuNode.GetIdAs(MenuIdType.Form).ToCssClass();

            // Set this attribute to String.Empty if you don't want form double submit prevention
            // This value is checked in jquery.bluechilli-mvc.js
            if (!attributes.ContainsKey("data-submitted")) attributes["data-submitted"] = "false";

            return htmlHelper.BeginForm(menuNode.Action, menuNode.Controller, FormMethod.Post, attributes);
        }

        /// <summary>
        /// Invokes the specified child action method with the specified parameters and returns the result as an HTML string.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="menuNode">The menu node to get the form Id, action and controller from.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the action.</param>
        /// <returns>HTML string for the action.</returns>
        public static MvcHtmlString Action<TModel>(this HtmlHelper<TModel> htmlHelper, MenuNode menuNode, object routeValues = null)
        {
            return htmlHelper.Action(menuNode.Action, menuNode.Controller, routeValues);
        }

        /// <summary>
        /// Returns HTML string for the Menu element.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="menuNode">The menu node to get the ViewData.</param>
        /// <param name="templateName">The template.</param>
        /// <returns>HTML string for the Menu element.</returns>
        public static MvcHtmlString Menu(this HtmlHelper htmlHelper, MenuNode menuNode, string templateName)
        {
            return CreateHtmlHelperForModel(htmlHelper, menuNode)
                .DisplayFor(m => menuNode, templateName);
        }

        /// <summary>
        /// Initializes a new instance of the HtmlHelper class.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="model">The model.</param>
        /// <returns>An instance of the System.Web.Mvc.HtmlHelper.<TModel></returns>
        public static HtmlHelper<TModel> CreateHtmlHelperForModel<TModel>(HtmlHelper htmlHelper, TModel model)
        {
            return new HtmlHelper<TModel>(htmlHelper.ViewContext, new ViewDataContainer<TModel>(model));
        }
    }

    /// <summary>
    /// Encapsulates the model's view data dictionary.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    internal class ViewDataContainer<TModel> : IViewDataContainer
    {
        /// <summary>
        /// Initializes a new instance of the ViewDataContainer class by using the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public ViewDataContainer(TModel model)
        {
            ViewData = new ViewDataDictionary<TModel>(model);
        }

        /// <summary>
        /// Initializes a new instance of the ViewDataDictionary class by using the specified model.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }
    }

    /// <summary>
    /// An enumeration of menu type.
    /// </summary>
    public enum MenuIdType
    {
        /// <summary>
        /// The specified menu is a modal window.
        /// </summary>
        Modal,
        /// <summary>
        /// The specified menu is an HTML form.
        /// </summary>
        Form
    }

    #region Default option classes
    /// <summary>
    /// Encapsulates link html attributes.
    /// </summary>
    public class HtmlLinkFieldOptions : HtmlDefaultFieldOptions
    {
        /// <summary>
        /// Initialises a new instance of BlueChilli.Web.HtmlLinkFieldOptions class.
        /// </summary>
        public HtmlLinkFieldOptions()
        {
            this.Title = string.Empty;
            this.IconClasses = string.Empty;
        }

        /// <summary>
        /// Gets or sets the title of the link.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the Id of the link.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets route values of the link.
        /// </summary>
        public object RouteValues { get; set; }
        /// <summary>
        /// Gets or sets CSS icon class names of the link.
        /// </summary>
        public string IconClasses { get; set; }
        /// <summary>
        /// Gets or sets CSS link class names of the link.
        /// </summary>
        public string LinkClasses { get; set; }
        /// <summary>
        /// Gets or sets the host name of the link.
        /// </summary>
        public string HostName { get; set; }
    }

    /// <summary>
    /// Encapsulates list item attributes.
    /// </summary>
    public class HtmlListItemFieldOptions : HtmlDefaultFieldOptions
    {
    }

    /// <summary>
    /// Encapsulates html attributes.
    /// </summary>
    public class HtmlDefaultFieldOptions
    {
        /// <summary>
        /// Gets or sets HTML attributes of the field.
        /// </summary>
        public object HtmlAttributes { get; set; }
    }

    /// <summary>
    /// This interface is intended to establish menu node visibility.
    /// </summary>
    public interface IMenuNodeVisibility
    {
        /// <summary>
        /// Checks whether the menu item is visible or not.
        /// </summary>
        /// <param name="Menu">The BlueChilli.Web.MenuNode.</param>
        /// <returns>True if menu item is visible, otherwise false.</returns>
        bool IsVisible(MenuNode Menu);
    }

    /// <summary>
    /// This interface allows the customisation of the menu status (active or inactive)
    /// </summary>
    public interface IMenuNodeStatus
    {
        /// <summary>
        /// Checks whether the menu item is active or not
        /// </summary>
        /// <param name="menu">The BlueChilli.Web.MenuNode.</param>
        /// <param name="routeValues"></param>
        /// <returns>True if menu item is active, otherwise false</returns>
        bool IsActive(MenuNode menu, object routeValues = null);
    }
    #endregion
}
