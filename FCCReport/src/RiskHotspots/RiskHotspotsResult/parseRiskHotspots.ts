import { IDropdownOption } from "@fluentui/react";
import { removeNamespaces } from "../../common";
import { Assembly, RiskHotspot } from "../../types";
import { allAssembliesOption } from "./filtering";
import { HotspotItem, MetricsWithStatusObject } from "./hotspotItem";

export function parseRiskHotspots(
  riskHotspots: RiskHotspot[],
  assemblies: Assembly[],
  namespacedClasses: boolean,
  standalone: boolean
): [
  items: HotspotItem[],
  metricNames: string[],
  assemblyFilterDropDownOptions: IDropdownOption<Assembly>[]
] {
  const items: HotspotItem[] = [];
  const metricNames: string[] = [];

  const assemblyFilterDropDownOptions: IDropdownOption<Assembly>[] = [
    allAssembliesOption,
  ];

  riskHotspots.forEach((riskHotspot) => {
    const assembly = assemblies[riskHotspot.assemblyIndex];
    // set up assembly filtering
    if (
      !assemblyFilterDropDownOptions.find((ddo) => {
        return ddo.data === assembly;
      })
    ) {
      assemblyFilterDropDownOptions.push({
        key: assembly.name,
        text: assembly.shortName,
        data: assembly,
      });
    }

    const _class = assembly.classes[riskHotspot.classIndex];
    const classPaths = _class.files.map((f) => f.path);
    const filePath = _class.files[riskHotspot.fileIndex].path;
    const methodMetric = riskHotspot.methodMetric;
    const metrics = methodMetric.metrics;
    const methodLine = methodMetric.line;
    const methodDisplay = methodMetric.shortName; // todo check the views - if necessary long form for method name

    const classDisplay = namespacedClasses
      ? _class.displayName
      : removeNamespaces(_class.displayName);

    const metricsWithStatusObject: MetricsWithStatusObject = {};
    riskHotspot.statusMetrics.forEach((statusMetric) => {
      const metric = metrics[statusMetric.metricIndex];
      const metricName = metric.name;
      metricsWithStatusObject[metricName] = {
        exceeded: statusMetric.exceeded,
        name: metricName,
        value: metric.value,
      };
      if (!metricNames.includes(metricName)) {
        metricNames.push(metricName);
      }
    });

    const key = `${filePath}${methodLine}`;
    const hotspotItem: HotspotItem = {
      key,
      assemblyDisplay: assembly.shortName,
      assembly,
      filePath,
      methodLine,
      methodDisplay,
      classDisplay,
      classPaths,
      metrics: metricsWithStatusObject,
      standalone,
    };
    items.push(hotspotItem);
  });

  return [items, metricNames, assemblyFilterDropDownOptions];
}
