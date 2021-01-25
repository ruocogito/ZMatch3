#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{

	if(texture(texture1, texCoord).a > 0.5)
	    outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord), 0.9);
	else
	    outputColor = texture(texture0, texCoord);
}
