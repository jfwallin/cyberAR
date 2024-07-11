import tkinter as tk
from tkinter import filedialog, ttk
import re

class LogParserGUI:
    def __init__(self, master):
        self.master = master
        self.master.title("Log Parser GUI")
        self.master.geometry("1200x800")

        self.host_log = ""
        self.peer_log = ""
        self.guids = set()

        self.create_widgets()

    def create_widgets(self):
        # File selection buttons
        self.host_button = tk.Button(self.master, text="Select Host Log", command=self.load_host_log)
        self.host_button.pack(pady=10)

        self.peer_button = tk.Button(self.master, text="Select Peer Log", command=self.load_peer_log)
        self.peer_button.pack(pady=10)

        # GUID selection
        self.guid_var = tk.StringVar()
        self.guid_combo = ttk.Combobox(self.master, textvariable=self.guid_var)
        self.guid_combo.pack(pady=10)
        self.guid_combo.bind("<<ComboboxSelected>>", self.search_guid)

        # Log display
        self.log_frame = tk.Frame(self.master)
        self.log_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        self.host_text = tk.Text(self.log_frame, wrap=tk.NONE)
        self.host_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        self.peer_text = tk.Text(self.log_frame, wrap=tk.NONE)
        self.peer_text.pack(side=tk.RIGHT, fill=tk.BOTH, expand=True)

    def load_host_log(self):
        filename = filedialog.askopenfilename(filetypes=[("Text Files", "*.txt")])
        if filename:
            with open(filename, 'r') as file:
                self.host_log = file.read()
            self.host_text.delete(1.0, tk.END)
            self.host_text.insert(tk.END, self.host_log)
            self.extract_guids()

    def load_peer_log(self):
        filename = filedialog.askopenfilename(filetypes=[("Text Files", "*.txt")])
        if filename:
            with open(filename, 'r') as file:
                self.peer_log = file.read()
            self.peer_text.delete(1.0, tk.END)
            self.peer_text.insert(tk.END, self.peer_log)
            self.extract_guids()

    def extract_guids(self):
        guid_pattern = r'\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b'
        self.guids = set(re.findall(guid_pattern, self.host_log + self.peer_log))
        self.guid_combo['values'] = list(self.guids)

    def search_guid(self, event):
        selected_guid = self.guid_var.get()
        self.highlight_text(self.host_text, selected_guid)
        self.highlight_text(self.peer_text, selected_guid)

    def highlight_text(self, text_widget, search_string):
        text_widget.tag_remove('highlight', '1.0', tk.END)
        start_pos = '1.0'
        while True:
            start_pos = text_widget.search(search_string, start_pos, stopindex=tk.END)
            if not start_pos:
                break
            end_pos = f"{start_pos}+{len(search_string)}c"
            text_widget.tag_add('highlight', start_pos, end_pos)
            start_pos = end_pos
        text_widget.tag_config('highlight', background='yellow')

if __name__ == "__main__":
    root = tk.Tk()
    app = LogParserGUI(root)
    root.mainloop()
