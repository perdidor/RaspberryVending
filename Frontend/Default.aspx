<%@ Page Title="Главная" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <%--<div id="templatemo_content_wrapper">
    
    	<div id="templatemo_content">
        
            <div id="main_column">
            
                <div class="section_w590">
                    <h3>Welcome to our WEBSITE!</h3>
                    
                     <img class="image_wrapper fl_image" src="images/templatemo_image_04.jpg" alt="image" />
                   
                    <p>This XHTML/CSS Template is provided by <a href="http://www.templatemo.com" target="_parent">TemplateMo.com</a> for free of charge. You may download, modify and apply this template for your websites. Credits go to <a href="http://www.photovaco.com" target="_blank">Free Photos</a> for photos,  <a href="http://shiftyj.deviantart.com/" target="_blank">shiftyj</a> for Photoshop brushes and <a href="http://www.smashingmagazine.com" target="_blank">Smashing Magazine</a> for icons.</p>
                    <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed et quam vitae ipsum vulputate varius vitae semper nunc. Quisque eget elit quis augue pharetra feugiat. Suspendisse turpis arcu, dignissim ac laoreet a, condimentum in massa.</p>
                    
                <div class="cleaner"></div>
                </div>
                
                <div class="cleaner_h30"></div>
                
                <div class="section_w590">
                
                  <h3>New Products</h3>
					
                     <img class="image_wrapper fr_image" src="images/templatemo_image_01.jpg" alt="image" />              
                   
                    <p>Suspendisse feugiat, augue ac tincidunt vestibulum, ligula orci tincidunt arcu, quis scelerisque ante magna quis urna. Ut quis est congue dui porttitor porta. Suspendisse eu erat at nunc luctus rhoncus ut ut purus. Duis mollis dignissim fringilla. In hac habitasse platea dictumst. Nullam orci ante, tempor ac blandit eget, tristique non neque. Pellentesque eu leo tortor. Mauris et lectus eget elit sodales mattis ac id ligula. Quisque fermentum malesuada felis at suscipit. Praesent quis gravida diam. Ut mattis porttitor massa.</p>    
                    
                    <div class="cleaner_h20"></div>
                    <ul class="list_01">
                        <li>Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. </li>
                        <li>Nulla facilisi. Phasellus posuere justo id nunc bibendum convallis.</li>
                        <li>Aliquam sed nisi nulla, sit amet commodo arcu.</li>
                        <li>Sed sagittis, mauris vel fringilla varius, lacus diam faucibus nisl, eu rutrum neque elit.</li>
                    </ul>
                     
                    <div class="button_01"><a href="#">Read more</a></div>    
                </div>
                
                <div class="cleaner"></div>
            </div> <!-- end of main column -->
            
            <div id="side_column">
            
                <div class="side_column_box">
                    <h3>News and Events</h3>
                    
                    <div class="news_section">
                    	<img class="image_wrapper" src="images/templatemo_image_02.jpg" alt="image" />
                        <h4>Etiam tempus tellus eget </h4>
                        <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas et ipsum sem, ut lobortis dui.  <a href="#">Read more...</a></p>
                    </div>
                    
                    <div class="news_section">
                    	<img class="image_wrapper" src="images/templatemo_image_03.jpg" alt="image" />
                        <h4>Nam quis aliquet quam</h4>
                        <p>Sed pharetra neque vel mauris auctor ornare. Maecenas urna lorem, consectetur eget consectetur id.<a href="#"> Read more...</a></p>
                    </div>
                    
                    <div class="button_01"><a href="#">View All</a></div>    
                </div>
                
                <div class="side_column_box">
                
                    <h3>Newsletter</h3>
                        <input type="text" value="Enter your email address..." name="q" size="10" class="inputfield" title="searchfield" onfocus="clearText(this)" onblur="clearText(this)" />
                        <input type="submit" name="Search" value="Search" class="submitbutton" title="Search" />
                    <br />
                    <br />
                </div>
            
            </div>
            <!-- end of side column -->
            
            <div class="divider"> </div>
            
             <div class="footer_box m_right">
             <div class="footer_bottom"></div>
                    	
            <h5>Lorem ipsum dolor</h5>
            
            <div class="footer_box_content">
       	    <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed et quam vitae ipsum vulputate varius vitae semper nunc.</p>
                <div class="cleaner_h10"></div>
		        <ul class="list_01">
                    <li>Proin nec molestie ipsum</li>
                    <li>Curabitur ut mattis urna</li>
                    <li><a href="http://www.flashmo.com/page/1" target="_parent">Free Flash Templates</a></li>
                  	<li>Integer sit amet tortor vel diam </li>
                </ul>
                
	            <div class="button_01"><a href="#">Read more</a></div>
			</div>            
 
        </div>
        
         <div class="footer_box m_right">
             <div class="footer_bottom"></div>
                    	
            <h5>Maecenas urna lorem</h5>
            
            <div class="footer_box_content">
                <p>Morbi dictum semper varius. Quisque nec purus erat, vitae sodales urna. Integer aliquam sapien vitae turpis .</p>
                <div class="cleaner_h10"></div>
                <ul class="list_01">
                    <li>Nullam porttitor tellus ut turpis</li>
                    <li>Mauris lobortis nisl id lorem</li>
                    <li>Vivamus at mi a sem aliquet</li>
                    <li>Etiam tempus tellus eget est</li>
                </ul>
                <div class="button_01"><a href="#">Read more</a></div>                
            </div>            
 
        </div>
        
         <div class="footer_box">
             <div class="footer_bottom"></div>
                    	
            <h5>Donec eleifend</h5>
            
            <div class="footer_box_content">
            	<p>Integer bibendum quam sagittis neque consectetur eleifend. Integer feugiat magna sit amet leo rhoncus elementum.</p>
                <div class="cleaner_h10"></div>
                <ul class="list_01">
                    <li>Proin nec molestie ipsum</li>
                    <li>Vivamus ornare ornare leo</li>
                    <li>Nulla porta vehicula pulvinar</li>
                    <li>Duis ac eros quam</li>
                </ul>
	            <div class="button_01"><a href="#">Read more</a></div>                
			</div>            
 
        </div>
            
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->--%>
</asp:Content>

