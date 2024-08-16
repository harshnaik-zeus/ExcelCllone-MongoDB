using CsvHelper;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class CsvChunkService
{
    private readonly int _chunkSize;

    public CsvChunkService(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public List<string> GetChunk(string filePath, int chunkIndex)
    {
        var chunk = new List<string>();
        var startLine = chunkIndex * _chunkSize;

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.Read();
            csv.ReadHeader();
            for (int i = 0; i < startLine && csv.Read(); i++) { }

            while (csv.Read() && chunk.Count < _chunkSize)
            {
                var row = csv.Parser.Record;
                chunk.Add(string.Join(",", row));
            }
        }

        return chunk;
    }
}
