import {
    customizable,
    IButtonProps,
    BaseButton,
    nullRender,
} from "@fluentui/react";
import React from "react";
import { getStyles } from "@fluentui/react/lib/components/Button/ActionButton/ActionButton.styles";

export const vsStyledActionButtonScope = "VsStyledActionButton";

@customizable(vsStyledActionButtonScope, ["theme", "styles"], true)
export class VsStyledActionButton extends React.Component<IButtonProps> {
    public render(): JSX.Element {
        const { styles, theme } = this.props;

        return (
            <BaseButton
                {...this.props}
                variantClassName="ms-Button--action ms-Button--command"
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion -- decorator ensures theme
                styles={getStyles(theme!, styles)}
                onRenderDescription={nullRender}
            />
        );
    }
}
