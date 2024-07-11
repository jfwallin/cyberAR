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

        # Occurrence menus
        self.menu_frame = tk.Frame(self.master)
        self.menu_frame.pack(fill=tk.X, padx=10, pady=10)

        self.host_menu_var = tk.StringVar()
        self.host_menu = ttk.Combobox(self.menu_frame, textvariable=self.host_menu_var, width=50)
        self.host_menu.pack(side=tk.LEFT, padx=(0, 5))
        self.host_menu.bind("<<ComboboxSelected>>", self.center_host_occurrence)

        self.peer_menu_var = tk.StringVar()
        self.peer_menu = ttk.Combobox(self.menu_frame, textvariable=self.peer_menu_var, width=50)
        self.peer_menu.pack(side=tk.RIGHT, padx=(5, 0))
        self.peer_menu.bind("<<ComboboxSelected>>", self.center_peer_occurrence)

        # Log display
        self.log_frame = tk.Frame(self.master)
        self.log_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        self.host_text = tk.Text(self.log_frame, wrap=tk.NONE)
        self.host_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        self.host_scrollbar = tk.Scrollbar(self.host_text)
        self.host_scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.host_text.config(yscrollcommand=self.host_scrollbar.set)
        self.host_scrollbar.config(command=self.host_text.yview)

        self.peer_text = tk.Text(self.log_frame, wrap=tk.NONE)
        self.peer_text.pack(side=tk.RIGHT, fill=tk.BOTH, expand=True)
        self.peer_scrollbar = tk.Scrollbar(self.peer_text)
        self.peer_scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.peer_text.config(yscrollcommand=self.peer_scrollbar.set)
        self.peer_scrollbar.config(command=self.peer_text.yview)

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
        self.update_occurrence_menus(selected_guid)

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

    def update_occurrence_menus(self, guid):
        host_occurrences = self.find_occurrences(self.host_text, guid)
        peer_occurrences = self.find_occurrences(self.peer_text, guid)

        self.host_menu['values'] = [f"Occurrence {i+1}: {o[:50]}..." for i, o in enumerate(host_occurrences)]
        self.peer_menu['values'] = [f"Occurrence {i+1}: {o[:50]}..." for i, o in enumerate(peer_occurrences)]

    def find_occurrences(self, text_widget, search_string):
        occurrences = []
        start_pos = '1.0'
        while True:
            start_pos = text_widget.search(search_string, start_pos, stopindex=tk.END)
            if not start_pos:
                break
            line_start = text_widget.index(f"{start_pos} linestart")
            line_end = text_widget.index(f"{start_pos} lineend")
            occurrence = text_widget.get(line_start, line_end)
            occurrences.append(occurrence)
            start_pos = text_widget.index(f"{start_pos}+1c")
        return occurrences

    def center_host_occurrence(self, event):
        self.center_occurrence(self.host_text, self.host_menu_var.get())

    def center_peer_occurrence(self, event):
        self.center_occurrence(self.peer_text, self.peer_menu_var.get())

    def center_occurrence(self, text_widget, occurrence):
        if not occurrence:
            return
        
        guid = self.guid_var.get()
        occurrence_number = int(occurrence.split(':')[0].split()[1]) - 1
        
        start_pos = '1.0'
        for _ in range(occurrence_number + 1):
            start_pos = text_widget.search(guid, start_pos, stopindex=tk.END)
            if not start_pos:
                break
            start_pos = text_widget.index(f"{start_pos}+1c")
        
        if start_pos:
            text_widget.see(start_pos)
            text_widget.mark_set(tk.INSERT, start_pos)
            text_widget.focus_set()

            # Center the view
            text_widget.update_idletasks()
            first_visible = text_widget.index("@0,0")
            last_visible = text_widget.index(f"@0,{text_widget.winfo_height()}")
            visible_lines = int(last_visible.split('.')[0]) - int(first_visible.split('.')[0])
            target_line = int(start_pos.split('.')[0])
            new_top_line = max(1, target_line - visible_lines // 2)
            text_widget.yview_moveto(float(new_top_line) / float(text_widget.index(tk.END).split('.')[0]))

if __name__ == "__main__":
    root = tk.Tk()
    app = LogParserGUI(root)
    root.mainloop()
