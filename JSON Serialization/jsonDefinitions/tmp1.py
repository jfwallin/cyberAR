#set up the helper routines to load the jsons
# written by J Wallin

def jsonFromFile(fname):
    f = open(fname,"r")
s = f.read()
f.close()
return json.loads(s)

def jsonToFile(fname):
    print(fname)

def listFiles(assetPath, printFiles):
    finalList = []
fullList = os.listdir(assetPath)
for f in fullList:
    if f.find("json") == len(f)-4:
        print(f)
finalList.append(f)
return finalList

def loadFileIntoDictionary(path, flist):
    jdata = {}
for f in flist:
    jdata[f] = jsonFromFile(path + f)
return jdata



def writeScene(myScene, verbose):
    # save it to an output file
outputFile = myScene["jsonFileName"]

print(os.path.isfile(outputPath + outputFile))

# if there is a file already there
print(outputPath + outputFile)
fileExists = os.path.isfile(outputPath + outputFile)

if verbose:
    while (fileExists or outputFile == "" ):

        print("This is a listing of files in this path: ")
flist = os.listdir(outputPath)
for f in flist:
    print(f)
yn = input("The file " + outputPath + outputFile +" already exists \n Do you want to overwrite it? ")
if yn == "y" or yn == "Y":
    fileExists = False
else:
    outputFile = input("What is the new name for our file? ")
fileExists = os.path.isfile(outputPath + outputFile)


print("\nWriting " + outputPath+outputFile + "\n\n")


# form the final scene as a string
myScene["jsonFileName"] = outputFile
finalJson = json.dumps(myScene, indent=4)
f = open(outputPath + outputFile, "w", encoding='utf-8')
f.write(json.dumps(myScene, ensure_ascii=False, indent=4))
f.close()


def createScene(moduleDescription, sceneData, olist, verbose=False):
    sceneData["objects"] = olist


# make a copy of the generic scene for the activity file
myScene = jdata['genericActivity.json'].copy()

# apply the module description data ot the sceneObject
myScene.update(moduleDescription)
myScene.update(sceneData)


# we can dump the scene to see what it looks like so far  
if verbose:
    print("This is the Scene\n\n\n")
print(json.dumps(myScene, indent=4))    

writeScene(myScene, verbose)    


# this is a code stub for merging two activity files together
def concatenateScenes( outputPath, outputFile, sceneList):

    print(outputPath, outputFile)
# open the new file
f = open(outputPath + outputFile, 'w', encoding='utf-8')

for i in range(len(sceneList)):
    print("  \n scene")
print(i)
currentScene = sceneList[i]
print(currentScene)

# read in the files
fn1 = outputPath + currentScene
s1 = jsonFromFile(fn1)

# Dump the file - but do NOT use any indents.  This will flatten
# the file into a single line.  We then write it out as a line with a newline character.
a1 = json.dumps(s1, ensure_ascii=False)
f.write(a1 + "\n")

# close it
f.close()

