   M S B A M L   `   `   `  ����J  FKinect Face API, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:Kinect4Kinect4  5   	       Kinect4.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:Kinect4     5   	     � Title$  Kinect Face Detection��5   	   $��650.223��6'   $��953.239��68   $��20(�6H    � 
ResizeMode$ CanMinimize=�6V   .��5	      � $��0,0,-6,-109q�6   $
��Left=�6    $	��953��6;   $	��721��6G   $��Bottom=�6T   .+�5
   
   �� -   5         ��Name$ 	FaceImage��5
      $
��Left=�6#   $	��417��6>   $��0q�6K   $	��Top=�6V   $	��758��6n   �� 5   
   -   5         ��Name$ Kinect��5      $��Kinect��6!   $��44��6G   $��Bottom=�6S   $
��Left=�6n   $	��115��6�   $��786,0,0,622q�6�   �� 5   
   -   5        $
 Save��5      $
��Save��6   $
��Left=�6.   $��786,134,0,0q�6I   $	��Top=�6^   $	��115��6v   $��44��6�   �� 5   
   -   5        $ Browse��5      $��Browse��6!   $
��Left=�62   $��786,206,0,0q�6M   $	��Top=�6b   $	��115��6z   $��44��6�   �� 5   
   -   5        $ Detect��5      $��Detect��6!   $
��Left=�62   $��786,278,0,0q�6M   $	��Top=�6b   $	��115��6z   $��44��6�   �� 5   
   -   5         ��Name$ Result��5      $
��Left=�6$   $��	0,422,0,0q�6?   $
��Wrap=�6R   $��	TextBlock��6f   $	��Top=�6w   $��28(�6�   $	��692��6�   $	��186��6�   5      5      