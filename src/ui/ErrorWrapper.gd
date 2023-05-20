extends Control

@export var accept_dialog_path : NodePath
@onready var accept_dialog : NativeAcceptDialog = get_node(accept_dialog_path)

func set_text(text : String):
	accept_dialog.dialog_text = text

func show_error():
	accept_dialog.show()

func hide_error():
	accept_dialog.hide()
