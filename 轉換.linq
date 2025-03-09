<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Text.Encodings.Web</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

class Question
{
    public string sn { get; set; }
    public string @class { get; set; }
    public string question { get; set; }
    public List<Option> options { get; set; }
    public string remark { get; set; }
    public string felo { get; set; }
    public string pic { get; set; }
}

class Option
{
    public string option { get; set; }
    public bool answer { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // 輸入、輸出檔案路徑
        string inputFilePath = @"C:\Users\zx304\OneDrive\桌面\TLE\TLE.txt";
        string outputFilePath = @"C:\Users\zx304\OneDrive\桌面\TLE\questions.json";

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("輸入的檔案不存在！");
            return;
        }

        int serialNumber = 1;
        List<Question> allQuestions = new List<Question>();
        string[] lines = File.ReadAllLines(inputFilePath);

        Question currentQuestion = null;
        bool isOptionSection = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // 若遇空行且目前題目已有選項，則視為題目結束
            if (string.IsNullOrEmpty(line) && currentQuestion != null && currentQuestion.options.Count > 0)
            {
                allQuestions.Add(currentQuestion);
                currentQuestion = null;
                continue;
            }

            // 遇到「題碼：」則代表新題目開始
            if (line.StartsWith("題碼："))
            {
                // 如果上一題已建立，先加入清單
                if (currentQuestion != null && currentQuestion.options.Count > 0)
                {
                    allQuestions.Add(currentQuestion);
                }

                // 建立新題目，並設定 SN 為三位數格式
                currentQuestion = new Question
                {
                    sn = serialNumber.ToString("D3"),
                    options = new List<Option>(),
                    remark = "",
                    felo = "",
                    pic = ""
                };
                serialNumber++;

                // 使用正規表達式解析題碼前綴與中括號內的分類
                var match = Regex.Match(line, @"題碼：(\w+)\[(.*?)\]");
                if (match.Success)
                {
                    string prefix = match.Groups[1].Value;
                    currentQuestion.@class = match.Groups[2].Value;
                    
                    // 下一行預期為題目內容（以「？」結尾），並將前綴加到題目前面
                    if (i + 1 < lines.Length)
                    {
                        string nextLine = lines[i + 1].Trim();
                        int questionEnd = nextLine.IndexOf('？');
                        if (questionEnd != -1)
                        {
                            currentQuestion.question = prefix + "; " + nextLine.Substring(0, questionEnd + 1);
                            isOptionSection = true;
                            i++; // 跳過題目內容行
                        }
                    }
                }
                continue;
            }

            // 處理選項行，假設選項以 (A)、(B)、(C)、(D) 開頭
            if (isOptionSection && (line.StartsWith("(A)") || line.StartsWith("(B)") ||
                line.StartsWith("(C)") || line.StartsWith("(D)")))
            {
                string optionText = line.Substring(4).Trim();
                currentQuestion.options.Add(new Option
                {
                    option = optionText,
                    answer = false
                });
                continue;
            }

            // 遇到「觀看解答」則表示選項區結束
            if (line == "觀看解答")
            {
                isOptionSection = false;
                continue;
            }

            // 處理答案行，例如「解答：A」
            if (line.StartsWith("解答："))
            {
                string answer = line.Replace("解答：", "").Trim();
                // 逐一檢查答案字串中的每個字母
                foreach (char c in answer)
                {
                    int answerIndex = "ABCD".IndexOf(c);
                    if (answerIndex >= 0 && answerIndex < currentQuestion.options.Count)
                    {
                        currentQuestion.options[answerIndex].answer = true;
                    }
                }
            }
        }

        // 若最後一題尚未加入清單，則補上
        if (currentQuestion != null && currentQuestion.options.Count > 0)
        {
            allQuestions.Add(currentQuestion);
        }

        // 將每個題目分別序列化，每筆 JSON 物件獨立一行，移除最外層陣列符號
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var jsonLines = allQuestions.Select(q => System.Text.Json.JsonSerializer.Serialize(q, jsonOptions));
        string jsonString = string.Join(Environment.NewLine, jsonLines);

        try
        {
            // 確保輸出目錄存在
            string outputDir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(outputFilePath, jsonString);
            Console.WriteLine($"轉換完成，已儲存至 {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"儲存檔案時發生錯誤：{ex.Message}");
        }
    }
}
