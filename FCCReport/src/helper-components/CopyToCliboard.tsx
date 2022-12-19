export function CopyToClipboard(props: { children: React.ReactNode }) {
    return (
        <span
            onKeyUp={(evt) => {
                if (evt.ctrlKey && evt.key === "c") {
                    const text = (evt.target as Element).textContent;
                    if (text !== null) {
                        navigator.clipboard
                            .writeText(text)
                            .catch(() =>
                                console.log("Could not clipboard.writetext")
                            );
                    }
                }
            }}
        >
            {props.children}
        </span>
    );
}
