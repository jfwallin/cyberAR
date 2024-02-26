
import json
import tkinter as tk
from tkinter import messagebox
import tkinter.scrolledtext
import copy
from tkScrolledFrame import VerticalScrolledFrame
from tkinter import filedialog as fd


class jsonEditor:
    def __init__(self, ds, theFrame, theWindow, parent=None, windowName=None):
        self.ds = ds
        self.dstmp = {}
        self.dsOriginal = copy.deepcopy(self.ds)
        self.theFrame = theFrame
        self.theWindow = theWindow
        self.theWindow.protocol("WM_DELETE_WINDOW", self.on_closing)
        self.viewingMode = 0
        self.lineDataOriginal = None
        self.parent = parent
        self.editingEnabled = True
        self.theWindowName = windowName

    def saveJson(self):

        if self.editingEnabled == False:
            return

        fn = fd.asksaveasfilename()
        try:

            f = open(fn, "w")
            f.write(json.dumps(self.ds, indent=4))
            f.close()
        except:
            messagebox.showerror("Error", "Error saving json file")

    def on_closing(self):

        if self.editingEnabled == False:
            return

        print("closing!!!!")
        if self.viewingMode == 0:

            self.grabNewLineData()
            self.ds = self.createNewDataStructure2(self.ds)
            keysOk = self.compareJsonKeys()
            print("keysOk", keysOk)

            # if keysOk == True:
            #    changesOk = self.verifyChangesInValues()
            # else:
            #    changesOk = False

            changesOk = True
            if changesOk == True:

                closeOK = messagebox.askyesnocancel(
                    "Exit Editor", "Do you want to: \n Yes - Exit and save your changes \n No - Exit and discard your changes \n Cancel the exit?")
                if closeOK == True:
                    print("sending the signal")
                    print("parent", self.parent)
                    self.parent.signalJsonEditorClosed(json.dumps(self.ds))
                    self.theWindow.destroy()
                elif closeOK == False:
                    self.parent.signalJsonEditorClosed()
                    self.theWindow.destroy()
                else:
                    pass

        if self.viewingMode == 1:
            jsonValid = self.validateJsonEdits()

            if jsonValid == True:
                self.updateDataFromJson()

                keysOk = self.compareJsonKeys()
                if keysOk == True:
                    changesOk = self.verifyChangesInValues()
                else:
                    changesOk = False

                if changesOk == True:
                    if messagebox.askokcancel("Quit", "Do you want to quit?"):
                        if self.parent != None:
                            print("sending the signal")
                            print("parent", self.parent)
                            self.parent.signalJsonEditorClosed(
                                json.dumps(self.ds))
                        self.theWindow.destroy()

    def openDict(self, kk, ll, d):
        print("openDict")

        self.thisEditKey = kk
        self.thisEditKeyData = ll
        self.editingEnabled = False

        dtmp = d.get("1.0", tk.END)
        dsLocalObject = json.loads(dtmp)

        theWindow = tk.Toplevel(self.theWindow)
        theWindow.title("JSON Editor")
        theWindow.geometry('1000x1300')
        theWindow.configure(background='white')

        ttheFrame = VerticalScrolledFrame(theWindow)
        ttheFrame.pack(fill=tk.BOTH, expand=True)
        theFrame = ttheFrame.interior
        jsObj = jsonEditor(dsLocalObject, theFrame, theWindow, self)
        jsObj.showModule2()

    def signalJsonEditorClosed(self, ds=None):

        # the idea is to figure out the key being used, and then figure out the
        # element number within the list.  The keyData has the starting and ending
        # of the elements in this list.  So, we just substract the actual line
        # number from the starting line number, and that gives us the index.
        print("signaled closed", self)

        if ds != None:
            aa = self.ds[self.thisEditKey]
            ii2 = self.keyData[self.thisEditKey][1][0]
            theIndex = int(self.thisEditKeyData) - ii2

            #print("old = ", aa[theIndex])
            self.ds[self.thisEditKey][theIndex] = json.loads(
                ds)  # this shoujld work!

        self.editingEnabled = True
        self.showModule2()

    def showChanges3(self):

        kdataOriginal, lineDataOriginal = self.parseDataStructure(
            self.dsOriginal)

        allKeys = list(set(list(self.ds.keys()) +
                       list(self.dsOriginal.keys())))
        deletedKeys = []
        addedKeys = []
        for k in allKeys:
            try:
                korg = kdataOriginal[k]
            except:
                korg = None

            try:
                knew = self.keyData[k]
            except:
                knew = None

            if korg == None:
                addedKeys.append(k)
            if knew == None:
                deletedKeys.append(k)
            # print(k, korg, knew)

        for i in range(len(self.data)):

            dType = self.lineData[i][2]
            newValue = self.data[i].get("1.0", tk.END).strip()
            try:
                oldValue = lineDataOriginal[i][1]
            except:
                oldValue = None

            # process the input string into the correct data type
            try:
                if dType == float:
                    newValue = float(newValue)
                elif dType == int:
                    newValue = int(newValue)
                elif dType == str:
                    snew = str(newValue)
                    if snew + "\n" == oldValue:  # fix \n line problem
                        snew = snew + "\n"
                    newValue = str(snew)
                elif dType == bool:
                    if newValue.find("T") > -1 or newValue.find("t") > -1:
                        newValue = True
                    else:
                        newValue = False
                    newValue = newValue
                else:
                    newValue = None

                if oldValue != newValue:
                    if oldValue == None:
                        self.data[i].configure(bg="lightpurple")
                    else:
                        self.data[i].configure(bg="lightblue")
                else:
                    self.data[i].configure(bg="white")
            except:
                self.data[i].configure(bg="red")

    # JSON EDIT FRAME

    def validateJson(self):

        jsonIsValid = self.validateJsonEdits()

        if jsonIsValid == True:
            keysOk = self.compareJsonKeys()
        else:
            keysOk = False

        if keysOk == True:
            changesOk = self.verifyChangesInValues()
        else:
            changesOk = False

        if changesOk == True:
            rstart = 0
            self.buildJsonFrame()

    def buildJsonFrame(self):
        text = json.dumps(self.ds, indent=4)
        ww = 70
        hh = 50
        self.text_area = tk.scrolledtext.ScrolledText(self.theFrame,
                                                      wrap=tk.WORD, width=ww, height=hh,
                                                      font=("Times New Roman", 12))
        self.text_area.insert(tk.INSERT, text)
        self.text_area.grid(row=1, column=0, columnspan=3,
                            sticky=tk.W+tk.E+tk.N+tk.S)
        return self.text_area

    def validateJsonEdits(self):
        windowData = self.text_area.get("1.0", tk.END)
        try:
            self.dstmp = json.loads(windowData)
        except:
            # popup window to show error and allow the user to reset the data or re-edit it
            retry = messagebox.askokcancel("askretrycancel",
                                           "There is an error in your json.  Click Ok to reset the data or Cancel to re-edit the json. ")

            if retry == False:  # reset the json
                print("duckdfd")
# self.showModule2(viewingMode=1)
            else:
                print("retry")
                self.ds = copy.deepcopy(self.dsOriginal)
                # self.viewingMode = 1
                self.showModule2()
            return False

        return True

    def updateDataFromJson(self):
        # load the data from the text window into dstmp
        windowData = self.text_area.get("1.0", tk.END)
        # rint("the window data", type(windowData))
        # print(windowData)
        # print("----")

        try:
            self.ds = json.loads(windowData)
            self.viewingMode = 0
            print("sdfjsdl")
            self.showModule2()
        except:
            print("error parsing json")
            messagebox.showerror(
                "Error", "Invalid JSON format - you can check the data and try again or use the Reset Raw Json option to reset the data")
            self.viewingMode = 0
            # self.showModule2()

    # Validation of data

    def compareJsonKeys(self):
        originalKeys = self.dsOriginal.keys()
        newKeys = self.ds.keys()

        missingKeys = []
        for k in originalKeys:
            if k not in newKeys:
                missingKeys.append(k)

        extraKeys = []
        for k in newKeys:
            if k not in originalKeys:
                extraKeys.append(k)

        s = ""
        if len(missingKeys) > 0:
            s = "The following keys are missing from the new json:\n"
            for k in missingKeys:
                s = s + "   -" + k + "\n"

        if len(extraKeys) > 0:
            s = s + "\n\nThe following keys have been added to the new json:\n"
            for k in extraKeys:
                s = s + "   -" + k + "\n"

        if len(s) > 0:
            s = s + "\n\nDo you wish to continue with the new json changes?"

            acceptChanges = messagebox.askyesno("Key Changes", s)

            if acceptChanges == False:
                s = "Do you wish to reset the json to the original data?"
                resetData = messagebox.askyesno("Reset the Data", s)
                if resetData == True:
                    self.ds = copy.deepcopy(self.dsOriginal)
                    self.showModule2()

                    return False
            else:  # reset the dats
                # self.showModule2()
                return True

        else:
            return True

    def checkNewKeyTypes(self):
        self.ds = self.ds
        self.dstmp = self.dstmp

        for k in self.dstmp.keys():
            if k in self.dsOriginal.keys():
                if type(self.dstmp[k]) != type(self.ds[k]):
                    s = "The key " + k + " has been changed from a " + str(type(self.ds[k])) + " to a " + str(
                        type(self.dstmp[k])) + ".  Do you wish to continue with the new json changes?"
                    acceptChanges = messagebox.askyesno("askyesno", s)
                    if acceptChanges == False:
                        s = "Do you wish to reset the json to the original data?"
                        resetData = messagebox.askyesno("askyesno", s)
                        if resetData == True:
                            self.viewingMode = 1
                            self.showModule2()

                            return False
                        else:
                            return True
        return True

    def verifyChangesInValues(self):

        s = ""
        for k in self.ds.keys():
            if k in self.dsOriginal.keys():
                if self.dsOriginal[k] != self.ds[k]:
                    s = s + "    -The key " + k + " has been changed from " + \
                        str(self.ds[k]) + " to " + \
                        str(self.dsOriginal[k]) + "\n"
                    changed = True

        if s != "":
            s = s + "\n\nDo you wish to continue with these changes?"
            acceptChanges = messagebox.askyesno("Accept New Values", s)

            if acceptChanges == False:
                s = "Do you wish to reset these to the original values?"
                resetData = messagebox.askyesno("Reset Values", s)
                if resetData == True:
                    # self.viewingMode = 1
                    self.ds = copy.deepcopy(self.dsOriginal)
                    self.showModule2()
                    return False
                else:
                    return False

            return True

    # newTypesOk = checkNewKeyTypes()

    # utilties on data structures

    def createNewDataStructure2(self, aa):
        # make new structure
        bb = {}
        r = 0
        for k in aa.keys():
            # print("processing key", k, "type", type(aa[k]))

            dType = self.keyData[k][0]
            if dType == float or dType == int or dType == str or dType == bool:
                bb[k] = self.castVariable(dType, self.newLineData[r][1])
                r = r + 1

            elif dType == list:
                newList = []
                for i in range(self.keyData[k][1][0], self.keyData[k][1][1]):
                    snew = self.newLineData[i][1]
                    sold = self.lineData[i][1]

                    print(snew, type(snew))
                    if snew == None:
                        snew = ""
                    if sold == None:
                        sold = ""
                    if type(snew) == str and type(sold) == str:
                        if snew + "\n" == sold:  # fix \n line problem
                            snew = snew + "\n"

                    newList.append(snew)
                    r = r + 1
                bb[k] = newList

            elif dType == dict:
                newDict = {}
                for i in range(self.keyData[k][1][0], self.keyData[k][1][1]):
                    if self.newLineData[i][0] != None:
                        newDict[self.newLineData[i][0]] = self.castVariable(
                            dType, self.newLineData[i][1])
                        r = r + 1
                bb[k] = newDict

        else:
            bb[k] = aa[k]

        return bb

    def castVariable(self, newType, value):

        try:
            if newType == float:
                return float(value)
            elif newType == int:
                return int(value)
            elif newType == str:
                return str(value)
            elif newType == bool:
                return bool(value)
            else:
                return value
        except:
            return value

    def parseDataStructure(self, theStructure):
        r = 0
        keyDataLocal = {}    # key data type, key data range
        lineDataLocal = []   # subindex, value, datatype

        for k in theStructure.keys():
            dType = type(theStructure[k])
            keyDataLocal[k] = [dType, [r, r+1]]
            if dType == float or dType == int or dType == str or dType == bool:
                lineDataLocal.append([None, theStructure[k], dType])
                r = r + 1

            if dType == list:
                keyDataLocal[k] = [dType, [r, r+len(theStructure[k])]]
                for i in range(len(theStructure[k])):
                    lineDataLocal.append([None, theStructure[k][i],
                                          type(theStructure[k][i])])
                    r = r + 1

            if dType == dict:
                ct = 0
                rstart = r
                for l in theStructure[k].keys():
                    lineDataLocal.append(
                        [l, theStructure[k][l], type(theStructure[k][l])])
                    r = r + 1
                    ct = ct + 1

                keyDataLocal[k] = [dType, [rstart, rstart+ct]]

        return keyDataLocal, lineDataLocal

    # Field data

    def showChanges2(self):

        for i in range(len(self.data)):

            dType = self.lineData[i][2]
            newValue = self.data[i].get("1.0", tk.END).strip()

            try:
                if dType == float:
                    newValue = float(newValue)
                elif dType == int:
                    newValue = int(newValue)
                elif dType == str:
                    snew = str(newValue)
                    sold = self.lineData[i][1]
                    if snew + "\n" == sold:  # fix \n line problem
                        snew = snew + "\n"
                    newValue = str(snew)
                elif dType == bool:
                    if newValue.find("T") > -1 or newValue.find("t") > -1:
                        newValue = True
                    else:
                        newValue = False
                    newValue = newValue
                else:
                    newValue = None

                if self.lineData[i][1] != newValue:
                    self.data[i].configure(bg="lightblue")
                else:
                    self.data[i].configure(bg="white")
            except:
                self.data[i].configure(bg="red")

    def resetFields(self):

        if self.editingEnabled == True:
            return

        for i in range(len(self.data)):
            oldData = self.lineData[i][1]
            oldDataType = self.lineData[i][2]
            if oldDataType == bool:
                if oldData == True:
                    oldData = "True"
                else:
                    oldData = "False"
            self.data[i].delete("1.0", tk.END)
            self.data[i].insert(tk.END, oldData)
            self.data[i].configure(bg="white")

    def grabNewLineData(self):

        self.newLineData = copy.deepcopy(self.lineData)
        for i in range(len(self.data)):
            self.newLineData.append(self.lineData[i])  # copy the original data

            dType = self.lineData[i][2]
            newValue = self.data[i].get("1.0", tk.END).strip()

            if 1 == 1:
                if dType == float:
                    newValue = float(newValue)
                elif dType == int:
                    newValue = int(newValue)
                elif dType == str:
                    newValue = str(newValue)
                elif dType == bool:
                    if newValue.find("T") > -1 or newValue.find("t") > -1:
                        newValue = True
                    else:
                        newValue = False
                    newValue = newValue
                elif dType == dict:
                    pass
                elif dType == list:
                    pass
                else:
                    newValue = None

            self.newLineData[i][1] = newValue

        return self.newLineData

    def buildEntryFrame(self, rstart=1, expandLists=False, specialOption=None):

        ###
        # global labels
        # global sublabels
        # global deleteList
        # global newFieldName
        # global newFieldValue
        # global currentOrder
        # global orderList

        print("specialOption = ", specialOption)
        if self.editingEnabled == False:
            specialOption = None

        if specialOption == None:
            infoColumn = 0
            labelColumn = 1
            sublabelColumn = 2
            dataColumn = 3
            openColumn = 4
        else:
            infoColumn = 0
            labelColumn = 1
            sublabelColumn = 2
            dataColumn = 3
            openColumn = 4

        rstart = 2
        rr = rstart
        labels = []
        sublabels = []

        self.deleteList = len(self.keyData.keys()) * [0]
        cb = []
        self.orderList = len(self.keyData.keys()) * [0]
        self.theOptions = list(range(len(self.keyData.keys())))
        self.currentOrder = copy.deepcopy(self.theOptions)

        self.openButtons = []

        self.data = []

        # dcount = 0
        keyCount = 0
        for k in self.keyData.keys():

            if specialOption == "Delete":
                self.deleteList[keyCount] = tk.IntVar()
                cb.append(tk.Checkbutton(self.theFrame, text=str(keyCount),
                                         variable=self.deleteList[keyCount], onvalue=1, offvalue=0,
                                         background='white'))
                cb[-1].grid(row=rr, column=infoColumn, sticky=tk.N)
                # print(k, keyCount, len(deleteList), len(cb))

            if specialOption == "Reorder":

                self.orderList[keyCount] = tk.IntVar()
                self.orderList[keyCount].set(self.theOptions[keyCount])
                cb.append(tk.OptionMenu(
                    self.theFrame, self.orderList[keyCount], *self.theOptions,
                    command=self.updateMenus))
                cb[-1].configure(width=15)
                self.orderList[keyCount].set(self.theOptions[keyCount])
                cb[-1].grid(row=rr, column=infoColumn, sticky=tk.EW)
                print(self.theOptions[keyCount], keyCount, len(
                    self.orderList), self.orderList[keyCount].get())

            keyType = self.keyData[k][0]
            # add the label for the key
            l = tk.Label(self.theFrame, text=k, background='white')
            l.grid(row=rr, column=labelColumn, sticky=tk.N)
            labels.append(l)
            keyCount = keyCount + 1

            # loop over items
            for l in range(self.keyData[k][1][0], self.keyData[k][1][1]):
                d = None
                dType = self.lineData[l][2]
                value = self.lineData[l][1]
                sublabel = self.lineData[l][0]
                if sublabel != None:
                    ww = 10
                    hh = 1
                    sl = tk.Text(self.theFrame,  height=hh,
                                 width=ww, background="white")
                    sl.insert(tk.END, str(sublabel))
                    sl.grid(row=rr, column=sublabelColumn, sticky=tk.N)
                    sublabels.append(sl)

                if dType == float or dType == int:
                    ww = 25
                    hh = 1
                    bval = str(value)
                    d = tk.Text(self.theFrame, height=hh, width=ww)
                    d.insert(tk.END, bval)
                    d.grid(row=rr, column=dataColumn, sticky=tk.W)
                    self.data.append(d)
                    rr = rr + 1
                elif dType == str:
                    bval = value
                    vlength = len(bval)
                    if vlength < 70:
                        ww = 70
                        hh = 1
                    else:
                        hh = int(vlength / 70) + 1
                        ww = 70
                    d = tk.Text(self.theFrame, height=hh, width=ww)
                    d.insert(tk.END, bval)
                    d.grid(row=rr, column=dataColumn, sticky=tk.W)
                    self.data.append(d)
                    rr = rr + 1
                elif dType == bool:
                    # bval = str(value)
                    if value == True:
                        bval = "True"
                    else:
                        bval = "False"
                    hh = 1
                    ww = 10
                    # d = tk.Checkbutton(self.theFrame, variable=bval)
                    d = tk.Text(self.theFrame, height=hh, width=ww)
                    d.insert(tk.END, bval)
                    d.grid(row=rr, column=dataColumn, sticky=tk.W)
                    self.data.append(d)
                    rr = rr + 1

                if dType == dict:
                    print("dict", sublabel, value, type(value))
                    hh = 1
                    ww = 10
                    if sublabel != None:
                        sl = tk.Text(self.theFrame,  height=hh, width=ww)
                        sl.insert(tk.END, str(sublabel))
                        sl.grid(row=rr, column=sublabelColumn, sticky=tk.N)
                        sublabels.append(sl)

                    #bval = str(value)
                    bval = json.dumps(value, indent=0)
                    print('string', bval, len(bval))
                    print("===")

                    ww = 70
                    hh = 8
                    #vlength = len(bval)
                    # if vlength < 70:
                    #    ww = 70
                    #    hh = 1
                    # else:
                    #    hh = int(vlength / 70) + 1
                    #    ww = 70
                    d = tk.Text(self.theFrame, height=hh, width=ww)
                    d.insert(tk.END, bval)
                    d.grid(row=rr, column=dataColumn, sticky=tk.W)
                    self.data.append(d)

                    self.openButtons.append(tk.Button(self.theFrame, text="Open",
                                                      command=lambda k=k, l=l, d=d: self.openDict(k, l, d)))
                    self.openButtons[-1].grid(row=rr,
                                              column=openColumn, sticky=tk.W)

                    rr = rr + 1

                # if expandLists == True:
                if 1 == 0:
                    if dType == dict:
                        ww = 10
                        hh = 1
                        sl = tk.Text(self.theFrame,  height=hh, width=ww)
                        sl.insert(tk.END, str(sublabel))
                        sl.grid(row=rr, column=sublabelColumn, sticky=tk.N)
                        sublabels.append(sl)

                        bval = str(value)
                        vlength = len(bval)
                        if vlength < 70:
                            ww = 70
                            hh = 1
                        else:
                            hh = int(vlength / 70) + 1
                            ww = 70
                        d = tk.Text(self.theFrame, height=hh, width=ww)
                        d.insert(tk.END, bval)

                    # if dType == list or dType == dict:
                    #    hh = 1
                    #    ww = 100
                    #    bval = ''
                    #    d = tk.Text(self.theFrame, height=hh, width=ww)
                    #    d.insert(tk.END, bval)
                    #    d.grid(row=rr, column=labelColumn, sticky=tk.W)
                    rr = rr + 1

                # if dType == list or dType == dict:
                #    # add the label for the key
                #    l = tk.Label(self.theFrame, text="", background="white")
                #    l.grid(row=rr, column=labelColumn, sticky=tk.N)

                #    rr = rr + 1

        if specialOption == "Delete":
            b = tk.Button(self.theFrame, text="Delete",
                          command=lambda: self.deleteKeys())
            b.grid(row=rr+2, column=0, sticky=tk.N)

        if specialOption == "Add" or specialOption == None:
            self.newFieldName = tk.Text(
                self.theFrame, width=20, height=1, background="white")
            self.newFieldName.grid(row=rr+4, column=labelColumn, sticky=tk.E)

            self.newFieldValue = tk.Text(
                self.theFrame, width=60, height=1, background="white")
            self.newFieldValue.grid(row=rr+4, column=dataColumn, sticky=tk.W)

            # l = tk.Label(self.theFrame, text="", background="white")
            # l.grid(row=rr+5, column=labelColumn, sticky=tk.N)

            b = tk.Button(self.theFrame, text="Add", command=self.addNewKey)
            b.grid(row=rr+6, column=labelColumn, sticky=tk.W)

            bc = tk.Button(self.theFrame, text="Cancel",
                           command=self.showModule2)
            bc.grid(row=rr+6, column=labelColumn+1, sticky=tk.W)

        if specialOption == "Reorder":
            b = tk.Button(self.theFrame, text="Accept New Order",
                          command=lambda: self.acceptNewOrder())
            b.grid(row=rr+6, column=labelColumn, sticky=tk.W)

            bc = tk.Button(self.theFrame, text="Cancel",
                           command=self.showModule2)
            bc.grid(row=rr+6, column=labelColumn+1, sticky=tk.W)

        return rr

    def addNewKey(self):
        if self.editingEnabled == False:
            return
        self.ds[self.newFieldName.get("1.0", tk.END).strip()] = self.newFieldValue.get(
            "1.0", tk.END).strip()
        self.showModule2()

    def deleteKeys(self):
        if self.editingEnabled == False:
            return
        keyCount = 0
        deleteKeyList = []
        for k in self.keyData.keys():
            if self.deleteList[keyCount].get() == 1:
                deleteKeyList.append(k)
            keyCount = keyCount + 1

        s = "The following keys will be deleted: \n"
        for k in deleteKeyList:
            s = s + "     -" + k + "\n"
        s = s + "Are you sure you want to delete these keys?"
        acceptDelete = tk.messagebox.askyesno("Delete the keys", s)
        if acceptDelete == True:
            for k in deleteKeyList:
                del self.ds[k]
        self.showModule2()

    def acceptNewOrder(self, theEvent=None):
        if self.editingEnabled == True:
            return
        # reorder the data structure
        ds1 = {}
        # make a list of all the keys on the form in the displayed order and the keys
        # that are not on the form
        otherKeys = []
        keys = []
        for k in self.ds.keys():
            if not k in self.keyData.keys():
                otherKeys.append(k)
            else:
                keys.append(k)

        newKeyOrder = []
        # loop through the index of the keys
        for i in range(len(keys)):
            # loop through the current order array
            for j in range(len(keys)):
                ii = self.currentOrder[j]
                # if the current row matches the index in the current order array
                if ii == i:
                    k = keys[j]
                    ds1[k] = self.ds[k]

        for k in otherKeys:
            ds1[k] = self.ds[k]

        self.ds = copy.deepcopy(ds1)
        # print(ds1)
        self.showModule2()

    def updateMenus(self, theEvent=None):
        oldList = copy.deepcopy(self.currentOrder)
        newList = []
        for i in range(len(self.orderList)):
            newList.append(self.orderList[i].get())

        update = self.reOrderList(oldList, newList)
        for i in range(len(self.orderList)):
            self.orderList[i].set(update[i])
            self.currentOrder[i] = update[i]

    def reOrderList(self, oldList, newList):
        # both list must be the same length
        assert len(oldList) == len(newList)

        # find the index of the change - both lists must be the same
        changeIndex = -1
        for i in range(len(oldList)):
            if oldList[i] != newList[i]:
                assert changeIndex == -1
                changeIndex = i
                oldValue = oldList[i]
                newValue = newList[i]

        update = []
        if changeIndex > -1:
            # update the lists
            if oldValue > newValue:
                for i, v in enumerate(newList):
                    if v >= newValue and i != changeIndex and v < oldValue:
                        v = v + 1
                    update.append(v)
            else:
                for i, v in enumerate(newList):
                    if v <= newValue and i != changeIndex and v > oldValue:
                        v = v - 1
                    update.append(v)
        else:
            for v in oldList:   # no changes, so return the original list
                update.append(v)
        return update

    '''
    def newJson(self):

        dn = 0
        for k in self.ds.keys():
            if k != 'objects' and k != 'clips':

                dt = type(self.ds[k])  # original data type
                oldData = self.ds[k]   # original data
                # new data from the cells
                newData = data[dn].get("1.0", tk.END)
                if dt == str:
                    newData = str(newData).strip()
                elif dt == int:
                    newData = int(newData)
                elif dt == float:
                    newData = float(newData)
                elif dt == bool:
                    if newData.find("T") > -1 or newData.find("t") > -1:
                        newData = True
                    else:
                        newData = False
                elif dt == list:
                    if len(newData) > 1:
                        newData = newData.strip()
                        oldData = oldData[0]
                    else:
                        newData = []

                if oldData != newData:
                    data[dn].configure(bg="lightblue")
                else:
                    data[dn].configure(bg="white")

                self.updatedData[k] = newData  # update the new data dictionary
                dn = dn + 1
        return self.updatedData
        '''

    # module display and utilties

    def donothing(self):
        print("Nothing to do here")

    def modifyData(self, ss=""):
        for widget in self.theFrame.winfo_children():
            widget.destroy()
        self.buildEntryFrame(rstart=1, expandLists=False, specialOption=ss)

    def modeSwitch(self, mode=0):
        print("mode switch -   new mode", mode, "  old mode", self.viewingMode)

        if mode == self.viewingMode:
            return
        else:
            if mode == 0:  # we are moving to fields, so serialize the json
                print("serialize the json")
                self.updateDataFromJson()
            else:  # we are moving to json, so parse the json
                self.grabNewLineData()
                self.dstmp = self.createNewDataStructure2(self.ds)
                self.viewingMode = mode
                self.ds = self.dstmp
                self.showModule2()

    def showModule2(self, displayOptions=None):

        print("viewing mode", self.viewingMode)
        rstart = 1
        self.keyData, self.lineData = self.parseDataStructure(self.ds)

        # FILE MENU OPTIONS
        menubar = tk.Menu(self.theWindow)
        filemenu = tk.Menu(menubar, tearoff=0)
        # filemenu.add_command(label="New", command=self.donothing)
        # filemenu.add_command(
        #    label="Open Module", command=lambda: self.openModule())
        # filemenu.add_command(
        #    label="Open JSON", command=lambda: self.openJson())

        # filemenu.add_command(label="Close", command=self.closeFile)
        filemenu.add_command(label="Export Json to File",
                             command=lambda: self.saveJson())
        # filemenu.add_command(label="Import Json from File",
        #                     command=lambda: self.donothing())

        filemenu.add_separator()
        filemenu.add_command(label="Close Window", command=self.on_closing)
        menubar.add_cascade(label="File", menu=filemenu)

        editmenu = tk.Menu(menubar, tearoff=0)
        if self.viewingMode == 0:
            editmenu.add_command(label="Validate changes",
                                 command=lambda: self.showChanges2())
            editmenu.add_command(label="Reset changes",
                                 command=lambda: self.resetFields())
            editmenu.add_command(label="Edit View",
                                 command=lambda: self.self.modifyData(""))
            # editmenu.add_command(label="Add", command=lambda: self.modifyData("Add"))
            editmenu.add_command(
                label="Delete", command=lambda: self.modifyData("Delete"))
            editmenu.add_command(
                label="Reorder", command=lambda: self.modifyData("Reorder"))

        elif self.viewingMode == 1:
            print("vm1")
            editmenu.add_command(label="Validate Changes",
                                 command=lambda: self.validateJson())
            editmenu.add_command(
                label="Reset changes", command=lambda: self.buildJsonFrame())
            editmenu.add_command(label="Update From Json",
                                 command=lambda: self.updateDataFromJson())
        menubar.add_cascade(label="Edit", menu=editmenu)

        viewmenu = tk.Menu(menubar, tearoff=0)
        viewmenu.add_command(label="Display Json",
                             command=lambda: self.modeSwitch(1))
        viewmenu.add_command(label="Display Fields",
                             command=lambda: self.modeSwitch(0))
        menubar.add_cascade(label="View", menu=viewmenu)

        self.theWindow.configure(menu=menubar)

        for widget in self.theFrame.winfo_children():
            widget.destroy()

        if self.theWindowName != None:
            ss = self.theWindowName
        else:
            ss = ""

        title = tk.Label(self.theFrame, text=ss,
                         font=("Arial Bold", 20), background="#ffffff")
        title.grid(column=0, row=0, columnspan=3, sticky=tk.NSEW)

        if self.viewingMode == 0:
            self.buildEntryFrame(specialOption=displayOptions)
        elif self.viewingMode == 1:
            self.text_area = self.buildJsonFrame()

    # MAIN CODE
if __name__ == "__main__":

    genericModule = {
        "moduleName": "",
        "specificName": "",
        "prefabName": "demoPrefab",
        "prerequisiteActivities": [],
        "educationalObjectives": [
            ""
        ],
        "instructions": [
            "click on something... anything!"
        ],
        "numRepeatsAllowed": 0,
        "numGradableRepeatsAllowed": 0,
        "gradingCriteria": "",
        "currentScore": 0.0,
        "bestScore": 0.0,
        "completed": False,
        "currentSubphase": 0,
        "subphaseNames": [],
        "urlJson": "",
        "json": "",
        "timeToEnd": 15.0,
        "endUsingButton": True,
        "objects": [
        ],
        "clips": []
    }

    theWindow = tk.Tk()
    theWindow.title("JSON Editor")
    theWindow.geometry('1000x1100')
    theWindow.configure(background='white')

    # theFrame = tk.Frame(theWindow, background="#ffffff",
    #                    width = 1000, height = 1000, padx = 15, pady = 5)
    # theFrame.pack(fill="both", expand=True, padx=20, pady=20)
    ttheFrame = VerticalScrolledFrame(theWindow)
    ttheFrame.pack(fill="both", expand=True)
    theFrame = ttheFrame.interior
    # jsObj = jsonEditor(ds, theFrame.interior, theWindow)

    jsObj = jsonEditor(genericModule, theFrame, theWindow)

    jsObj.showModule2()
    # buildEntryFrame(self, rstart=1, expandLists=False, specialOption=None)
    tk.mainloop()


# reorder doesn't work
# switching between json and fields doesn't preserve chagnes
# closing doesn't return the json file
