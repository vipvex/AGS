using UnityEngine;
using System.Collections;

public static class ColorExtensions {

    public static Color ParseColor (string col) 
    {

        //Takes strings formatted with numbers and no spaces before or after the commas:
        // "1.0,1.0,.35,1.0"
        var strings = col.Split(","[0] );
        Color output = new Color();

        for (var i = 0; i < 4; i++) 
        {
            output[i] = System.Single.Parse(strings[i]);
        }

        return output;
    }
 
}
