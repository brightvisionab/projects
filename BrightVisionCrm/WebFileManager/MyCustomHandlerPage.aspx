
<%@ Page Title="WebDAV" Language="C#" AutoEventWireup="true"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
  <head>
    <title>IT Hit WebDAV Server Engine</title><meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
	<script type="text/javascript">
	    var port = window.location.port;
	    if (port == "")
	        port = window.location.protocol == 'http:' ? '80' : '443'; // Web Folders on Windows XP require port, even if it is a default port 80 or 443.
	    var webDavFolderUrl = window.location.protocol + '//' + window.location.hostname + ':' + port + '<%=Request.ApplicationPath.TrimEnd('/')%>/';

	    function init() {
	        if (navigator.appName == "Microsoft Internet Explorer")
	            oBrowseWindowsExplorer.disabled = false;
	    }
	    function OpenWebFolder() {
	        var res = oViewFolder.navigate(webDavFolderUrl);
	        if (res != "OK") {
	            oAddress.innerText = webDavFolderUrl;
	            oError.innerText = res;
	            oInfo.style.display = "";
	        }
	    }


	    // JavaScript file, styles and images required to run Ajax File Browser are loaded from IT Hit website. 
	    // To load files from your website download them here: http://www.webdavsystem.com/ajaxfilebrowser/download, 
	    // deploy them to your website and replace the path below.
	    var ajaxFilesUrl = "http://www.ajaxbrowser.com/ITHitService/";

	    function OpenAjaxFileBrowserWindow() {

	        // here we create a new browser window and put minimum HTML content required to run the Ajax File Browser.  
	        var ajaxFileBrowserHtml = "<!DOCTYPE html PUBLIC ' - //W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'>"
            + "<html>"
            + "<head>"
            + "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />"
            + "<title>IT Hit AJAX File Browser</title>"
            + "<style type='text/css'>"
            + "@import '" + ajaxFilesUrl + "AjaxFileBrowser/themes/ih_vista/include.css';"
            + "html, body { margin: 0px; padding: 0px; width: 100%; height: 100%; }"
            + "</style>"
            + "<script src='" + ajaxFilesUrl + "AjaxFileBrowser/ITHitAJAXFileBrowser.js' type='text/javascript'><\/script>"
            + "<script type='text/javascript'>" + InitAjaxFileBrowser.toString() + "<\/script>"
            + "</head>"
            + "<body onload='InitAjaxFileBrowser();'><div id='AjaxFileBrowserContainer' class='ih_vista' style='width: 100%; height: 100%'>Loading...</div></body>"
            + "</html>";

	        var win = window.open("", "", "menubar=1,location=1,status=1,scrollbars=1,resizable=1,width=900,height=600");
	        win.document.write(ajaxFileBrowserHtml);
	        win.document.close();
	    }

	    // This function creates Ajax File Browser and is added into opened window HTML.
	    function InitAjaxFileBrowser() {

	        // Customize the look of Ajax File Browser below
	        // http://www.webdavsystem.com/ajaxfilebrowser/programming/settings_reference
	        var settings = {
	            Id: 'AjaxFileBrowserContainer',     // (required) ID of the HTML control in which Ajax File Browser will be created
	            Url: window.opener.webDavFolderUrl, // (required) the root folder to be displyed in Ajax File browser
	            Style: 'height: 100%; width: 100%', // (required) always provide size of the control
	            FileIconsPath: window.opener.ajaxFilesUrl + 'AjaxFileBrowser/icons/',       // path to the folder where file icons are located
	            MsOfficeTemplatesPath: window.opener.webDavFolderUrl + 'Templates/',        // path to MS Office templates, always specify full path including domain: http://server/path/
	            SelectedFolder: window.opener.location.href,                                // folder to be selected, same as SetSelectedFolder call
	            PluginsPath: window.opener.ajaxFilesUrl + 'AjaxFileBrowser/plugins/',       // path to Java applet for opening documents in FF, Chrome and Safari
	            ShowFolders: true,                                                  // show/hide folders in Files View panel
	            ViewMode: ITHit.WebDAV.Client.AjaxFileBrowser.Render.ViewModes.Details, // View mode: Details / Medium / Large / ExtraLarge
	            ShowImagePreview: true,                                             // show thumbnail for images
	            ImageTypes: 'png|jpg|jpeg|gif',                                     // show thumbnails for this image types
	            NotAllowedCharacters: '\/:*?"<>|',                                  // the list of characters that file and folder names can not contain
	            ProgressRefreshTime: 5,                                             // progress refresh time for IE and legacy user agents in seconds
	            Panels: {
	                Toolbar: { Show: true },
	                AddressBar: { Show: true },
	                Folders: { Show: true },
	                UploadPanel: { Show: true },
	                FilesView: {
	                    Show: true,
	                    Order: ['Icon', 'Name', 'Size', 'Type', 'DateModified'],
	                    Columns: [
                            {
                                Id: 'Icon',
                                Show: true
                            },
                            {
                                Id: 'Name',
                                //Text: 'My Caption',
                                //Tooltip: 'My Tooltip',
                                //CellStyles: { textAlign: 'right', color: 'blue', fontWeight: 'bold' },
                                //HeaderStyles: { textAlign: 'right' },
                                Width: '300px'
                            },
                            {
                                Id: 'Size',
                                Show: true
                            },
                            {
                                Id: 'Type',
                                Show: true
                            },
                            {
                                Id: 'DateModified',
                                Show: true
                            }
                        ]
	                },
                    UploadProgressPanel: {
                        Show: true,
                        Order: ['Icon', 'Source', 'Destination'],
                        Columns: [
                        {
                            Id: 'Icon',
                            Show: true
                        },
                        {
                            Id: 'Source',
                            Show: false
                        },
                        {
                            Id: 'Destination',
                            //Width: 'auto'
                            Show: true
                        },
                        {
                            Id: 'ProgressBar',
                            Show: true,
                            Width: '180px'
                        },
                        {
                            Id: 'Progress',
                            Show: true
                        },
                        {
                            Id: 'Uploaded',
                            Show: true
                        },
                        {
                            Id: 'FileSize',
                            Show: true
                        },
                        {
                            Id: 'Speed',
                            Show: true
                        },
                        {
                            Id: 'TimeLeft',
                            Show: true,
                            Noresize: 'true'
                        },
                        {
                            Id: 'TimeElapsed',
                            Show: true
                        }
                        ]
                    }
                }
            };

	        var ajaxFileBrowser = new ITHit.WebDAV.Client.AjaxFileBrowser.Controller(settings);
	    }


	    function OpenTestsWindow() {
	        var ajaxFileBrowserHtml = "<html>"
            + "<head>"
            + "<title>IT Hit WebDAV AJAX Library Integration Tests</title>"
            + "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />"
            + "<script src='" + ajaxFilesUrl + "WebDAVAJAXLibrary/ITHitWebDAVClient.js' type='text/javascript'><\/script>"
            + "<link href='" + ajaxFilesUrl + "WebDAVAJAXLibrary/Tests/main.css' rel='stylesheet' type='text/css' />"
            + "<script src='" + ajaxFilesUrl + "WebDAVAJAXLibrary/Tests/TestsUI.js' type='text/javascript'><\/script>"
            + "<script src='" + ajaxFilesUrl + "WebDAVAJAXLibrary/Tests/IntegrationTests.js' type='text/javascript'><\/script>"
            + "<script type='text/javascript'>" + CreateTests.toString() + "<\/script>"
            + "</head>"
            + "<body onload='CreateTests();'></body>"
            + "	<div style='width: 100%;' id='testsHeader'></div>"
            + "	<div style='width: 100%; height: 80%; overflow-y: scroll;' id='tests'></div>"
            + "</html>";

	        var width = Math.round(screen.width * 0.5);
	        var height = Math.round(screen.height * 0.8);
	        var win = window.open("", "", "menubar=1,location=1,status=1,scrollbars=1,resizable=1,width=" + width + ",height=" + height);
	        win.document.write(ajaxFileBrowserHtml);
	        win.document.close();
	    }

	    // This function runs tests and is added into opened window HTML.
	    function CreateTests() {
	        var url = window.opener.location.href;
	        var tests = new TestsControl(Tests, url, 'tests', 'testsHeader');
	        tests.RunAll();
	    }
	</script>
    <style type="text/css">
        body { font-family: Verdana; font-size:smaller; }
        li { padding-bottom: 7px; }
        input {width: 250px}
    </style>
</head>
	<body onload="init()">
	    <h1>IT Hit WebDAV Server Engine v<%=System.Reflection.Assembly.GetAssembly(typeof(ITHit.WebDAV.Server.DavEngine)).GetName().Version %></h1>
	    <p>This page is displayed when user hits any folder on your WebDAV server in a web browser. You can customize this page to your needs. To test your WebDAV server you can run Ajax integration tests right from this page:</p>
        <p><input type="button" onclick="OpenTestsWindow()" value="Run Integration Tests" /></p>
        <br />
        <p>Here are some ways of managing files on your WebDAV server:</p>
        <ul>
        <li>Use a WebDAV client provided with almost any OS. Refer to <a href="http://www.webdavsystem.com/server/documentation/access">Accessing WebDAV Server</a> page for detailed instructions. If you are running Internet Explorer you can open Web Folders / Mini-Redirector right from a web page:
		<p><input id="oBrowseWindowsExplorer" type="button" onclick="OpenWebFolder()" value="Browse using Windows Explorer" disabled="true" /></p>
        <span id="oViewFolder" style="behavior:url(#default#httpFolder)"></span>
		<div id="oInfo" style="display: none">
		    <H2>Failed to open folders view.</H2>
		    <p>Address: <font id="oAddress"></font></p>
		    <p>Error: <font id="oError"></font></p>
		    <p>Please install <a href="http://www.microsoft.com/downloads/details.aspx?familyid=17C36612-632E-4C04-9382-987622ED1D64&displaylang=en">Software Update for Web Folders.</a></p>
		</div>        
        </li>
        <li>Use the IT Hit Ajax File Browser. You can <a href="http://www.webdavsystem.com/ajaxfilebrowser/programming/">deploy</a> all files required for Ajax File Browser UI to your website, or you can reference necessary files from IT Hit website or if you cannot modify any files on your WebDAV server <a href="http://www.ajaxbrowser.com/?CrossDomainDemo">use the Cross-Domain access</a>.
        <p><input type="button" onclick="OpenAjaxFileBrowserWindow()" value="Browse using Ajax File Browser" /></p>
        </li>
        <li>Modify <a href="http://www.webdavsystem.com/server/next_version/doc/customization">your custom GET handler</a> to display content of your server. This is the fast way to list files on your WebDAV server.</li>
        </ul>
        
	</body>
</html>
