using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace Immanuel.Ffmpeg.Controllers
{
    public class FileController : ApiController
    {
        [HttpPost]
        public string ConvertToVids()
        {
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
            if (hfc.Count < 1)
            {
                throw new ApplicationException("Empty File Bad Request");
            }
            var srcfile = GetFile(Path.Combine(GetDirectory(), hfc[0].FileName), 0);
            string totype = System.Web.HttpContext.Current.Request.Form["tofmt"];
            string srctype = (Path.GetExtension(srcfile) ?? "").Replace(".", "");
            string tofile = Path.ChangeExtension(srcfile, totype);
            hfc[0].SaveAs(srcfile);
            string args = GetArgs(srcfile, tofile, srctype, totype);
            ProcessExecute(args);
            var tt = System.Web.HttpUtility.UrlEncode(@"~/App_data/Temp/" + Path.GetFileName(tofile));
            return tt;
        }

        [HttpGet]
        public HttpResponseMessage GetFiles(string pth)
        {
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath(pth);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            byte[] arr = File.ReadAllBytes(pPath);
            response.Content = new StreamContent(new MemoryStream(arr));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            response.Content.Headers.ContentLength = arr.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(pth)
            };

            return response;
        }

        string GetDirectory()
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            string tdir = (DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString());
            string fPath = Path.Combine(path, tdir);
            if (!Directory.Exists(fPath))
            {
                Directory.CreateDirectory(fPath);
            }
            return fPath;
        }

        string GetFile(string fpath, int seq)
        {
            string i = "_" + seq.ToString();
            string fpath1 = Path.Combine(Path.GetDirectoryName(fpath), Path.GetFileNameWithoutExtension(fpath) + i.ToString() + Path.GetExtension(fpath));
            if (!File.Exists(fpath1))
            {
                return fpath1;
            }
            else
            {
                return GetFile(fpath, ++seq);
            }
        }

        static string ProcessExecute(string args)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = Path.Combine(path, "ffmpeg", "ffmpeg.exe");
                proc.StartInfo.Arguments = args;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                //Thread.Sleep(500);
                proc.Start();
                proc.WaitForExit();
                StringBuilder q = new StringBuilder();
                while (!proc.HasExited)
                {
                    q.Append(proc.StandardOutput.ReadToEnd());
                }
                string r = q.ToString();
                proc.Close();
                return r;
            }
        }

        static string GetArgs(string srcfile, string tofile, string stype, string ttype)
        {
            string args = "";
            if (stype.ToLower() == "avi" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -c:v libx264 -crf 19 -preset slow -c:a aac -b:a 192k -ac 2 \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec libx264 -pix_fmt yuv420p -profile:v baseline -preset slow -crf 22 -movflags +faststart \"" + tofile + "\"";
            return args;
        }
    }
}
