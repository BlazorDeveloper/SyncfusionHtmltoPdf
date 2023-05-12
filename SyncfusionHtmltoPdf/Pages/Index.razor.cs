using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.PdfViewer;
using Syncfusion.Blazor.PdfViewerServer;
using Syncfusion.Blazor.Popups;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using Syncfusion.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace SyncfusionHtmltoPdf.Pages
{
    public partial class Index
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        IWebHostEnvironment environment { get; set; }
        SfDialog pdfViewerDialog;
        SfPdfViewerServer pdfViewerServer;
        public bool IsPdfViewerDialogModelVisible { get; set; } = false;
        public string DocumentPath { get; set; }
        public PdfViewerToolbarSettings ToolbarSettings = new PdfViewerToolbarSettings()
        {
            ToolbarItems = new()
            {
                Syncfusion.Blazor.PdfViewer.ToolbarItem.PageNavigationTool,
                Syncfusion.Blazor.PdfViewer.ToolbarItem.MagnificationTool,
                Syncfusion.Blazor.PdfViewer.ToolbarItem.SearchOption,
                Syncfusion.Blazor.PdfViewer.ToolbarItem.PrintOption,
                Syncfusion.Blazor.PdfViewer.ToolbarItem.DownloadOption
            }
        };
        private byte[] pdfBuffer;

        private DotNetObjectReference<Index> dotNetObject { set; get; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                if (firstRender)
                {

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                dotNetObject = DotNetObjectReference.Create<Index>(this);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        protected async Task CreateSyncfusionPdf()
        {
            IsPdfViewerDialogModelVisible = true;
            await InvokeAsync(StateHasChanged);
            string htmlCode = await GetHtml();
            #region Syncfusion
            //Initialize HTML to PDF converter 
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            WebKitConverterSettings settings = new WebKitConverterSettings();

            //Set WebKit path
            settings.WebKitPath = Path.Combine(environment.ContentRootPath, "QtBinariesWindows");
            int viewportWidth = 1366;
            int viewportHeight = 0;
            settings.WebKitViewPort = new Size(viewportWidth, viewportHeight);
            settings.Margin = new Syncfusion.Pdf.Graphics.PdfMargins { Top = 30, Left = 0, Right = 0, Bottom = 30 };

            //Initialize the WebKit converter settings           
            //Disable WebKit warning message
            settings.DisableWebKitWarning = true;
            //Assign WebKit settings to HTML converter
            htmlConverter.ConverterSettings = settings;

            //Convert URL to PDF
            PdfDocument document = htmlConverter.Convert(htmlCode, "");

            //Save and close the PDF document 
            MemoryStream Stream = new MemoryStream();
            document.Save(Stream);

            document.Close(true);
            await SaveFile(Stream.ToArray());
            //pdfBuffer = Stream.ToArray();
            document.Dispose();
            htmlConverter = null;
            settings = null;

            #endregion

        }

        protected async Task SaveFile(byte[] stream)
        {
            try
            {
                using (MemoryStream ms = new())
                {

                    DocumentPath = environment.WebRootPath + "//SitePDF//" + DateTime.Now.ToString("yyyyMMdd-HHMMss_fff") + ".pdf";
                    ms.Position = 0;
                    using (var fs = new FileStream(DocumentPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        //fs.Lock(0, fs.Length);
                        ms.CopyTo(fs);
                        using (var writer = new BinaryWriter(fs))
                        {
                            writer.Write(stream, 0, stream.Length);
                            await writer.DisposeAsync();
                            writer.Close();
                        }
                    }
                    ms.Dispose();
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<string> GetHtml()
        {
            string message = await JSRuntime.InvokeAsync<string>("GetHtml");
            return message;
        }

    }
}
