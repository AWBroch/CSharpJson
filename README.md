# CSharpJson

A simple JSON parser in C#

| file                     | implementation   | time (secs) | memory (KB) |
|:------------------------:|:----------------:|:-----------:|:-----------:|
| tests/ascii_strings.json | CSharpJson       | 0.488       | 190688      |
| tests/ascii_strings.json | System.Text.Json | 0.159       | 128832      |
| tests/ascii_strings.json | Newtonsoft       | 0.299       | 295440      |
| tests/food.json          | CSharpJson       | 0.094       | 37392       |
| tests/food.json          | System.Text.Json | 0.084       | 37664       |
| tests/food.json          | Newtonsoft       | 0.115       | 45168       |
| tests/geojson.json       | CSharpJson       | 0.382       | 100528      |
| tests/geojson.json       | System.Text.Json | 0.178       | 77840       |
| tests/geojson.json       | Newtonsoft       | 0.412       | 199360      |
| tests/numbers.json       | CSharpJson       | 0.699       | 152000      |
| tests/numbers.json       | System.Text.Json | 0.358       | 115264      |
| tests/number.json        | Newtonsoft       | 0.740       | 368656      |
| tests/random.json        | CSharpJson       | 1.277       | 284032      |
| tests/random.json        | System.Text.Json | 0.504       | 192976      |
| tests/random.json        | Newtonsoft       | 1.255       | 598576      |

A [Broch Web Solutions](https://www.brochweb.com/) project.
