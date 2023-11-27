using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;

namespace SilverlightCSOM
{
    public class CSOMHelper
    {
        private const string SITE = "http://mySite";
        private const string PAGE = "/default.aspx";

        public static void UpdateTitle(Object state)
        {
            ListItemHelper item = (ListItemHelper)state;

            using(ClientContext ctx = new ClientContext(SITE))
            {
                // Get the default page for the site
                File file = ctx.Web.GetFileByServerRelativeUrl(PAGE);

                // Get the WebPart manager to locate all the WebParts
                LimitedWebPartManager wpm = file.GetLimitedWebPartManager(PersonalizationScope.Shared);

                // Load all WebPart definitions found on the page
                IEnumerable<WebPartDefinition> definitions = ctx.LoadQuery(wpm.WebParts.Include(w => w.Id, w => w.WebPart));

                // Use the synchronous method here since the 
                // method is being called in separate thread
                ctx.ExecuteQuery();

                // Find the definition for the webpart to be updated
                WebPartDefinition def = definitions.FirstOrDefault(d => d.Id == item.ID);
                if(def != null)
                {
                    def.WebPart.Title = item.Title;
                    def.MoveWebPartTo(item.Zone.ToLower(), 0);
                    // Save the changes
                    def.SaveWebPartChanges();
                    // Commit
                    ctx.ExecuteQuery();
                }
            }
        }

        public static void DeleteWebPart(Object state)
        {
            ListItemHelper item = (ListItemHelper)state;

            using(ClientContext ctx = new ClientContext(SITE))
            {
                // Get the default page for the site
                File file = ctx.Web.GetFileByServerRelativeUrl(PAGE);

                // Get the WebPart manager to locate all the WebParts
                LimitedWebPartManager wpm = file.GetLimitedWebPartManager(PersonalizationScope.Shared);

                // Load all WebPart definitions found on the page
                IEnumerable<WebPartDefinition> definitions = ctx.LoadQuery(wpm.WebParts.Include(w => w.Id, w => w.WebPart));

                // Use the synchronous method here since the 
                // method is being called in separate thread
                ctx.ExecuteQuery();

                // Find the definition for the webpart to be deleted
                WebPartDefinition def = definitions.FirstOrDefault(d => d.Id == item.ID);
                if(def != null)
                {
                    def.DeleteWebPart();
                    // Save the changes
                    def.SaveWebPartChanges();
                    // Commit
                    ctx.ExecuteQuery();
                }
            }
        }

        public static void AddWebPart()
        {
            using(ClientContext ctx = new ClientContext(SITE))
            {
                File file = ctx.Web.GetFileByServerRelativeUrl(PAGE);
                LimitedWebPartManager wpm = file.GetLimitedWebPartManager(PersonalizationScope.Shared);

                WebPartDefinition newWpd = wpm.ImportWebPart(webpartXml);
                wpm.AddWebPart(newWpd.WebPart, "Right", 1);
                ctx.ExecuteQueryAsync(null, null);
            }
        }

        private static string webpartXml =
        @"<WebPart xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
            xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns='http://schemas.microsoft.com/WebPart/v2'>
              <Title>Content Editor</Title>
              <FrameType>Default</FrameType>
              <Description>Allows authors to enter rich text content.</Description>
              <IsIncluded>true</IsIncluded>
              <ZoneID>Left</ZoneID>
              <PartOrder>0</PartOrder>
              <FrameState>Normal</FrameState>
              <Height />
              <Width />
              <AllowRemove>true</AllowRemove>
              <AllowZoneChange>true</AllowZoneChange>
              <AllowMinimize>true</AllowMinimize>
              <AllowConnect>true</AllowConnect>
              <AllowEdit>true</AllowEdit>
              <AllowHide>true</AllowHide>
              <IsVisible>true</IsVisible>
              <DetailLink />
              <HelpLink />
              <HelpMode>Modeless</HelpMode>
              <Dir>Default</Dir>
              <PartImageSmall />
              <MissingAssembly>Cannot import this Web Part.</MissingAssembly>
              <PartImageLarge>/_layouts/images/mscontl.gif</PartImageLarge>
              <IsIncludedFilter />
              <Assembly>Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Assembly>
              <TypeName>Microsoft.SharePoint.WebPartPages.ContentEditorWebPart</TypeName>
              <ContentLink xmlns='http://schemas.microsoft.com/WebPart/v2/ContentEditor' />
              <Content xmlns='http://schemas.microsoft.com/WebPart/v2/ContentEditor'><![CDATA[​This is fun]]></Content>
              <PartStorage xmlns='http://schemas.microsoft.com/WebPart/v2/ContentEditor' />
            </WebPart>";
    }
}
