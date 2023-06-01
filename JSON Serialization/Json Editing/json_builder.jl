# Make sure the JSON package is installed
using Pkg
Pkg.add("JSON")
# Load the JSON package
using JSON

# Read in raw json
raw_json_str = read("transmission_demo.json", String)

# The JSON is currently a list of activity modules. They need to be stringified and put into the labdatainfo json

# Match the start and end of a module,
# Where there are curly braces at the very start of the line
pattern = r"^\{|^\}"m
matches = eachmatch(pattern, raw_json_str)
println("Num Matches = $(length(collect(matches))/2)")
for match in first(eachmatch(pattern, raw_json_str), 10)
    println("Match = $(match.match) at $(match.offset)")
end

# Extract the individual modules into a list
modules_json = []
for match_couple in Iterators.partition(matches, 2)
    pretty_formatted_module = raw_json_str[match_couple[1].offset:match_couple[2].offset]
    lines = split(pretty_formatted_module, "\n")
    clean_lines = strip.(lines)
    single_line_module = join(clean_lines, "")
    push!(modules_json, single_line_module)
end

# Build the labinfo json using a Dict
lab_info = Dict(
    "Lab_ID" => "Transmission Demo",
    "Author" => "John Wallin",
    "CourseName" => "Introduction to Astronomy",
    "EstimatedLength" => "10 minutes",
    "NumModules" => "3",
    "Objectives" => [
        "Learn to identify moon phases",
        "Test that Transmission Interactions are working"
    ],
    "ActivityModules" => modules_json,
    "Assets" => [],
    "Transmission" => "true"
)

# Write the labinfo json to a file
open("transmission_demo_labinfo.json", "w") do f
    JSON.print(f, lab_info)
end