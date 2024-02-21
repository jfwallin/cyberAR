using JSON

labjson = JSON.parsefile("Full_Lab_Transmission.json")
modulesjson = [JSON.parse(activityjson) for activityjson in labjson["ActivityModules"]]
obj_changes = [[[modification for modification in clip["objectChanges"]] for clip in mod["clips"]] for mod in modulesjson]

unique_names = []
for i in 1:length(obj_changes)
    for j in 1:length(obj_changes[i])
        for k in 1:length(obj_changes[i][j])
            if obj_changes[i][j][k]["name"] ∉ unique_names
                push!(unique_names, obj_changes[i][j][k]["name"])
            end
        end
    end
end

println(unique_names)

