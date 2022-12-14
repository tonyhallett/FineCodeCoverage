export function CopyToClipboard(props: { children: any }) {
    return (
        <span
            onKeyUp={(evt) => {
                if (evt.ctrlKey && evt.key === "c") {
                    const text = (evt.target as Element).textContent;

                    navigator.clipboard.writeText(text!);
                }
            }}
        >
            {props.children}
        </span>
    );
}
