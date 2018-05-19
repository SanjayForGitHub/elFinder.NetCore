using System;
using System.Threading.Tasks;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("el-finder/file-system")]
    public class FileSystemController : Controller
    {
        [Route("connector")]
        public async Task<IActionResult> Connector(string folderPath = "", string folderName = "Files")
        {
            var connector = GetConnector(folderPath, folderName);
            return await connector.Process(Request);
        }

        [Route("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnail(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector(string folderPath = "", string folderName = "Files")
        {
            var driver = new FileSystemDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);
            folderPath = folderPath != "" ? ("/" + folderPath) : "";
            var root = new RootVolume(
                Startup.MapPath($"~/Files{folderPath}"),
                $"http://{uri.Authority}/Files/{folderPath}",
                $"http://{uri.Authority}/el-finder/file-system/thumb/")
            {
                //IsReadOnly = !User.IsInRole("Administrators")
                IsReadOnly = false, // Can be readonly according to user's membership permission
                Alias = folderName, // Beautiful name given to the root/home folder
                MaxUploadSizeInKb = 500, // Limit imposed to user uploaded file <= 500 KB
                //LockedFolders = new List<string>(new string[] { "Folder1" })
            };

            driver.AddRoot(root);

            return new Connector(driver);
        }
    }
}