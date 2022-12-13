export default function getFontFamily(fontFamily:string):string{
    if(fontFamily.indexOf(' ')>-1){
        return `"${fontFamily}"`;
    }
    return fontFamily;
}