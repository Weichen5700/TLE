<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>System.Data.SqlClient</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

class Program
{
    static void Main()
    {
        string inputFilePath = @"C:\Users\zx304\Desktop\新增資料夾\20250310_TLE-main\TLE-main\uploadfile.txt"; // 輸入檔案路徑
        string outputFilePath = @"C:\Users\zx304\Desktop\新增資料夾\20250310_TLE-main\TLE-main\output.txt"; // 輸出檔案路徑

        List<string> invalidEntries = new List<string>();

        try
        {
            foreach (string line in File.ReadLines(inputFilePath))
            {
                try
                {
                    var item = JsonConvert.DeserializeObject<QuestionItem>(line);
                    if (item?.Options == null || item.Options.Count != 4 || !item.Options.Any(o => o.Answer))
                    {
                        invalidEntries.Add($"{item?.Sn} - {item?.Question}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"解析 JSON 失敗: {ex.Message}");
                }
            }

            if (invalidEntries.Count > 0)
            {
                File.WriteAllLines(outputFilePath, invalidEntries);
                Console.WriteLine($"不符合條件的結果已輸出至 {outputFilePath}");
            }
            else
            {
                Console.WriteLine("所有 JSON 資料皆符合條件！");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"讀取或寫入檔案時發生錯誤: {ex.Message}");
        }
    }
}

class QuestionItem
{
    [JsonProperty("sn")]
    public string Sn { get; set; }

    [JsonProperty("question")]
    public string Question { get; set; }

    [JsonProperty("options")]
    public List<OptionItem> Options { get; set; }
}

class OptionItem
{
    [JsonProperty("option")]
    public string Option { get; set; }

    [JsonProperty("answer")]
    public bool Answer { get; set; }
}
