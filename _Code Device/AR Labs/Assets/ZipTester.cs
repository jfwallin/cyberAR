using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class ZipTester : MonoBehaviour
{
    public string zipFilename;
    private string zipFilepath;
    private ZipArchive archive;

    private bool extractAllAtOnce;

    void Start()
    {
        // Set up zip filepath
        zipFilepath = Path.Combine(Application.persistentDataPath, zipFilename);
        if (!File.Exists(zipFilepath) || !zipFilename.EndsWith(".zip"))
            Debug.LogError("Incorrect zip filepath");
        Debug.Log($"zip filepath: {zipFilepath}");

        // Decide how to extract
        if(extractAllAtOnce)
            ZipFile.ExtractToDirectory(zipFilepath, Application.persistentDataPath);
        else
        {
            FileStream zipstream = new FileStream(zipFilepath, FileMode.Open);
            archive = new ZipArchive(zipstream);

            string filenames = "";

            foreach(ZipArchiveEntry entry in archive.Entries)
            {
                filenames += entry.FullName + "\n";
                FileInfo entryfi = new FileInfo(Path.Combine(
                    Application.persistentDataPath,
                    entry.FullName));
                Debug.Log($"entry name: --{entry.Name}--");
                Debug.Log($"entry fullname: {entry.FullName}");
                Debug.Log($"entry file info fullname: {entryfi.FullName}");
                entryfi.Directory.Create();
                if(entry.Name.Length != 0)
                    entry.ExtractToFile(entryfi.FullName);
            }
            archive.Dispose();
            Debug.Log($"files in zip archive: {filenames}");
        }
    }
}
