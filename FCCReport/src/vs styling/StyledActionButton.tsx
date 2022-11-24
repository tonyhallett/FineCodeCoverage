import { customizable, IButtonProps, BaseButton, nullRender } from "@fluentui/react";
import React from "react";
import { getStyles } from '@fluentui/react/lib/components/Button/ActionButton/ActionButton.styles';

@customizable('StyledActionButton', ['theme', 'styles'], true)
export class StyledActionButton extends React.Component<IButtonProps, {}> {
  public render(): JSX.Element {
    const { styles, theme } = this.props;

    return (
      <BaseButton
        {...this.props}
        variantClassName="ms-Button--action ms-Button--command"
        styles={getStyles(theme!, styles)}
        onRenderDescription={nullRender}
      />
    );
  }
}
 