   M S B A M L   `   `   `  ���� C  ?SCOI.WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:SCOI.WPFSCOI.WPF      SCOI.WPF.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:SCOI.WPF       � Title$  
MainWindow��$	��450��$	��800��.��]� .+��� $	��Top=� �� Orientation$ 
Horizontal=�.+�o� .@�m� $
��File��.@�m� $��Save As�� m� Command �� *SaveCommand+�  � RowDefinitions �� �� $	��100�� � ColumnDefinitions �� �� $	��300��.+��� $��0��$��0������ *OutputBitmap+�� $��1��$��0������ *Layers+ �� SelectedItem �� *CurrentLayer+���� .R��� $ Vertical=�$	��280��.+��� ���� *Path+�� $ 
Horizontal=�$	��280��.+��� $	��240������ *Path+�� $ Vertical=�$	��240��.+��� $��^��$��40��$��60������  �� Path$ DataContext.UpCommand� �� RelativeSource ��  �� AncestorType
 Window���� �� $��^��$��40��$��60��	 J� RenderTransformOrigin$	 0.5, 0.5&�z��� 
 �� ScaleY$
 -1Q���  �� DisplayMemberPath$
 Name������ $ DataContext.Methods� �� 
 Window �� SelectedValue �� *OverlayMethod+�� $ 
Horizontal=�.+��� $��R�� d� 	IsChecked �� *R+�� $��G�� �� *G+�� $��B�� �� *B+�� $ 
Horizontal=�.+��� $��Offset: ���� ���� *	OffsetX+�� $��x���� ���� *	OffsetY+�� -    ��Name$ 	addButton��$��1��$��1��$��+��$	��300��$	��100������ $ 
AddCommand�