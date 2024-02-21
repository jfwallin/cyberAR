# Read in host log and peer log files
hostlogs = readlines("HostLog.txt")
peerlogs = readlines("PeerLog.txt")

# Filter lines in host log to ones containing "msg guid"
hostlogs = filter(x -> occursin("msg guid", x), hostlogs)
# Filter lines in peer log to ones containing "message guid"
peerlogs = filter(x -> occursin("message guid", x), peerlogs)

# Parse the guids from the lines, after "guid: " and to the end
hostguids = map(x -> split(x, "guid: ")[2], hostlogs)
peerguids = map(x -> split(x, "message guid: ")[2], peerlogs)

# check that the guids are the same
println(Set(hostguids) == Set(peerguids))

length(hostguids)
length(peerguids)