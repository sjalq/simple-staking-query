module FileFormats

open System
open System.Numerics
open System.IO

let toFile map =
    let timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")
    let filePath = sprintf "output/results %s.csv" timestamp

    File.WriteAllLines(
        filePath,
        map
        |> Map.map (fun k v -> sprintf "%A,%A" k v)
        |> Map.values
    )

    let merkleFormat =
        {| 
            decimals = 18 
            airdrop = map
        |}

    // write the config.json file
    let merkleFormatJson = Newtonsoft.Json.JsonConvert.SerializeObject(merkleFormat, Newtonsoft.Json.Formatting.Indented)
    let filePath = "output/config.json"
    File.WriteAllText(filePath, merkleFormatJson)

    let ts = @"// Types
type IConfig = {
  decimals: number;
  airdrop: Record<string, number>;
};

const json = << add your config here >>

function convertJsonToIConfig(json: any): IConfig {
  return {
    decimals: Number(json.decimals),
    airdrop: json.airdrop
  };
}

const config = convertJsonToIConfig(json);

// Export config
export default config;
"

    // write the config.ts file
    let merkleFormatTs = ts.Replace("<< add your config here >>", merkleFormatJson)
    let filePath = "output/config.ts"
    File.WriteAllText(filePath, merkleFormatTs)