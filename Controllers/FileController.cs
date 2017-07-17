using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Immanuel.Ffmpeg.Controllers
{
    public class FileController : ApiController
    {
        static int GlobalCnt = 0;
        [HttpPost]
        public HttpResponseMessage ConvertToVids()
        {
            ++GlobalCnt;
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
            if (hfc.Count < 1)
            {
                throw new ApplicationException("Empty File Bad Request");
            }
            string totype = System.Web.HttpContext.Current.Request.Form["tofmt"];
            string srctype = (Path.GetExtension(hfc[0].FileName) ?? "").Replace(".", "");
            var srcfile = GetFile(Path.Combine(GetDirectory(srctype, totype), hfc[0].FileName), 0);
            string tofile = Path.ChangeExtension(srcfile, totype);
            hfc[0].SaveAs(srcfile);
            string args = GetArgs(srcfile, tofile, srctype, totype);
            ProcessExecute(args);
            var datedir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(tofile)));
            var tt = System.Web.HttpUtility.UrlEncode(@"~/App_data/Temp/" + datedir + @"/" + srctype + "_" + totype + @"/" + Path.GetFileName(tofile));
            return new HttpResponseMessage()
            {
                Content = new JsonContent(new
                {
                    Path = tt,
                    FileName = Path.ChangeExtension(hfc[0].FileName, totype),
                    TotCnt = GlobalCnt
                })
            };
        }

        [HttpGet]
        public HttpResponseMessage GetCnt()
        {
            return new HttpResponseMessage()
            {
                Content = new JsonContent(new
                {
                    TotCnt = GlobalCnt
                })
            };
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

        string GetDirectory(string srctype,string totype)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            string tdir = (DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString());
            string fPath = Path.Combine(path, tdir, srctype + "_" + totype);
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
            //All mp4
            if (stype.ToLower() == "avi" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -c:v libx264 -crf 19 -preset slow -c:a aac -b:a 192k -ac 2 \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy  \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec libx264 -pix_fmt yuv420p -profile:v baseline -preset slow -crf 22 -movflags +faststart \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -b 5000k -acodec wmav2 -vcodec wmv2 -ar 44100 -ab 56000 -ac 2 -y \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            // ALL AVI
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec libx264 -pix_fmt yuv420p -profile:v baseline -preset slow -crf 22 -movflags +faststart \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -b 5000k -acodec wmav2 -vcodec wmv2 -ar 44100 -ab 56000 -ac 2 -y \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL WMV
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL MPG
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL MOV
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL M4V
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL MkV
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL FLV
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            return args;
        }
    }

    public class JsonContent : HttpContent
    {

        private readonly MemoryStream _Stream = new MemoryStream();
        public JsonContent(object value)
        {

            Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var jw = new JsonTextWriter(new StreamWriter(_Stream));
            jw.Formatting = Formatting.Indented;
            var serializer = new JsonSerializer();
            serializer.Serialize(jw, value);
            jw.Flush();
            _Stream.Position = 0;

        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _Stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _Stream.Length;
            return true;
        }
    }
}
