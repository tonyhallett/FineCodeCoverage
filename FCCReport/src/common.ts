export function removeNamespaces(namespacedClass:string){
    const lastIndex = namespacedClass.lastIndexOf('.');
    return namespacedClass.substring(lastIndex+1);
}