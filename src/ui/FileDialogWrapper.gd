extends Control

@export var file_dialog_path : NodePath
@onready var file_dialog : NativeFileDialog = get_node(file_dialog_path)

func show_dialog():
	file_dialog.show()

func hide_dialog():
	file_dialog.hide()

func set_saving(saving : bool):
	if saving:
		file_dialog.file_mode = NativeFileDialog.FILE_MODE_SAVE_FILE
	else:
		file_dialog.file_mode = NativeFileDialog.FILE_MODE_OPEN_FILE

func connect_selected(callable : Callable):
	file_dialog.connect("file_selected", callable)
