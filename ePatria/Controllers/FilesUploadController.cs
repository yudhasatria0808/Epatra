using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.IO;
using System.Linq;
using System.Web;

namespace ePatria.Controllers
{
    [Authorize]
    public class FilesUploadController : Controller
    {
        string subPath = "~/Content/uploads/";
        public bool getFiles(string name, out List<string> outNewFilesName, out List<string> outPaths, UrlHelper url, HttpServerUtilityBase server)
        {
            bool path1exist = System.IO.Directory.Exists(server.MapPath(subPath));
            List<string> newFilesName = new List<string>();
            List<string> paths = new List<string>();
            if (path1exist)
            {
                string[] file1Names = Directory.GetFiles(server.MapPath(subPath));
                List<string> relatedFiles = file1Names.Where(p => p.Split('[')[1].Split(']')[0].Equals(name)).ToList();
                foreach (string fName in relatedFiles)
                {
                    string newFName = fName.Split(new char[] { '\\' }).Last();
                    newFilesName.Add(newFName);
                    string path = url.Content(subPath + "/" + newFName);
                    paths.Add(path);
                }
            }
            outNewFilesName = newFilesName;
            outPaths = paths;
            return true;
        }

        public bool addFile(string name, int i, HttpPostedFileBase file, HttpServerUtilityBase server)
        {
            var fileName = "[" + name + "]File" + i + Path.GetExtension(file.FileName);
            bool pathexist = System.IO.Directory.Exists(server.MapPath(subPath));
            if (!pathexist)
                System.IO.Directory.CreateDirectory(server.MapPath(subPath));
            var path = Path.Combine(server.MapPath(subPath), fileName);
            file.SaveAs(path);
            return true;
        }

        public int getLastNumberOfFiles(string name, HttpServerUtilityBase server)
        {
            string[] fileNames = Directory.GetFiles(server.MapPath(subPath));
            List<string> filesx = fileNames.Where(p => p.Split('[')[1].Split(']')[0].Equals(name)).OrderByDescending(p => p).ToList();
            string lastFile = filesx.Count() > 0 ? filesx.FirstOrDefault().ToString().Split('.')[0].Last().ToString() : "0";
            int lastNumber = Convert.ToInt32(lastFile);
            return lastNumber;
        }

        public bool deleteFiles(List<string> deletedFiles, HttpServerUtilityBase server)
        {
            foreach (var deletedFile in deletedFiles)
            {
                var fullPath = server.MapPath(deletedFile);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            return true;
        }
    }
}
